using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class AccountMgrViewModel : ObservableRecipient
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountEditWindowFactory _accountEditWindowFactory;
    public ResourceDictionaryService ResourceDictionaryService { get; }

    public ObservableCollection<WstAccount> Accounts { get; } = new();

    private WstAccount? _selectedAccount;
    private readonly StatusBarService _statusBarService;

    public WstAccount? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            SetProperty(ref _selectedAccount, value); 
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public bool IsSelected => SelectedAccount != null;

    public IRelayCommand AddCmd { get; }
    public IRelayCommand EditCmd { get; }
    public IRelayCommand RemoveCmd { get; }

    private async void UpdateAccounts()
    {
        Accounts.Clear();
        _statusBarService.Loading = true;
        var accounts = await _accountRepository.FindAllAsync();
        accounts.ForEach(Accounts.Add);
        _statusBarService.Loading = false;

    }

    public AccountMgrViewModel(IAccountRepository accountRepository, 
        IAccountEditWindowFactory accountEditWindowFactory,
        ResourceDictionaryService resourceDictionaryService,
        StatusBarService statusBarService)
    {
        _accountRepository = accountRepository;
        _accountEditWindowFactory = accountEditWindowFactory;
        ResourceDictionaryService = resourceDictionaryService;
        _statusBarService = statusBarService;
        UpdateAccounts();
        AddCmd = new RelayCommand(() =>
        {
            ShowEditDialog(EditMode.Create);
        });
        EditCmd = new RelayCommand(() =>
        {
            ShowEditDialog(EditMode.Edit, SelectedAccount);
        });
        RemoveCmd = new RelayCommand(() =>
        {
            if (SelectedAccount == null) return;
            _accountRepository.Delete(SelectedAccount.Email);
            var account = Accounts.First(a => a.Email == SelectedAccount.Email);
            Accounts.Remove(account);
        });

    }

    public void ShowEditDialog(EditMode mode, WstAccount? account = null)
    {
        var wnd = _accountEditWindowFactory.Create(mode, account);
        wnd.ViewModel.CloseCommand = new RelayCommand(() =>
        {
            UpdateAccounts();
            wnd.Close();
        });
        wnd.ShowDialog();
    }
}