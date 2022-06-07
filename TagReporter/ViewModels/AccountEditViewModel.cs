using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class AccountEditViewModel : ObservableRecipient
{
    private readonly IAccountRepository _accountRepository;
    public ResourceDictionaryService ResourceDictionaryService { get; }

    public string? Title { get; set; }
    public string? AddEditBtnContent { get; set; }

    public string? Email { get; set; }
    public string? Password { get; set; }
    private WstAccount? _account;
    private DialogMode _mode;

    public DialogMode Mode
    {
        get => _mode;
        set
        {
            SetProperty(ref _mode, value);
            OnPropertyChanged(nameof(IsCreateMode));
        }
    }

    public bool IsCreateMode => _mode == DialogMode.Create;

    public IRelayCommand? AddEditCommand { get; set; }
    public IRelayCommand? CloseCommand { get; set; }

    public AccountEditViewModel(IAccountRepository accountRepository, ResourceDictionaryService resourceDictionaryService)
    {
        _accountRepository = accountRepository;
        ResourceDictionaryService = resourceDictionaryService;
    }

    public void SetMode(DialogMode mode, WstAccount? account)
    {
        _account = account ?? new WstAccount();
        Mode = mode;

        switch (mode)
        {
            case DialogMode.Edit:
                Title = AddEditBtnContent = ResourceDictionaryService["Edit"] ?? "Edit";
                Email = _account.Email;
                Password = _account.Password;
                AddEditCommand = new RelayCommand(async () =>
                {
                    if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                    {
                        MessageBox.Show(
                            $"{ResourceDictionaryService["EmailOrPasswordCannotBeEmpty"] ?? "EmailOrPasswordCannotBeEmpty"}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    try
                    {
                        await _accountRepository.Update(Email, new WstAccount(Email!, Password!));
                        CloseCommand?.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
                break;
            case DialogMode.Create:
                Title = AddEditBtnContent = ResourceDictionaryService["Add"] ?? "Add";
                AddEditCommand = new RelayCommand(async () =>
                {
                    if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                    {
                        MessageBox.Show(
                            $"{ResourceDictionaryService["EmailOrPasswordCannotBeEmpty"] ?? "EmailOrPasswordCannotBeEmpty"}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    try
                    {
                        await _accountRepository.Create(new WstAccount(Email!, Password!));
                        CloseCommand?.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
                break;
            default:
                throw new Exception("Illegal mode!");
        }
    }
}