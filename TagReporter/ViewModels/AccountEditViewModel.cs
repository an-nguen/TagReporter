using ReactiveUI;
using System;
using System.Reactive;
using System.Windows;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class AccountEditViewModel: ReactiveObject
    {
        private AccountRepository _accountRepository { get; } = new();

        public string? Title { get; set; }
        public string? AddEditBtnContent { get; set; }
        
        public string? Email { get; set; }
        public string? Password { get; set; }
        private Account? _account;

        public ReactiveCommand<AccountEditWindow, Unit> AddEditCommand { get; }
        public ReactiveCommand<AccountEditWindow, Unit> CloseCommand { get; }

        public AccountEditViewModel(EditMode mode, Account? account)
        {
            var resource = CommonResources.GetLangResourceDictionary();
            _account = account ?? new Account();
            CloseCommand = ReactiveCommand.Create<AccountEditWindow>((wnd) => wnd.Close());

            switch (mode)
            {
                case EditMode.Edit:
                    Title = AddEditBtnContent = resource["Edit"].ToString() ?? "Edit";
                    Email = _account.Email;
                    Password = _account.Password;
                    AddEditCommand = ReactiveCommand.Create<AccountEditWindow>((wnd) =>
                    {
                        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                        {
                            MessageBox.Show(
                                $"{resource["EmailOrPasswordCannotBeEmpty"].ToString() ?? "EmailOrPasswordCannotBeEmpty"}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        try
                        {
                            _accountRepository.Update(_account, new Account(Email!, Password!));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        wnd.Close();
                    });
                    break;
                case EditMode.Create:
                    Title = AddEditBtnContent = resource["Add"].ToString() ?? "Add";
                    AddEditCommand = ReactiveCommand.Create<AccountEditWindow>((wnd) =>
                    {
                        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                        {
                            MessageBox.Show(
                                $"{resource["EmailOrPasswordCannotBeEmpty"].ToString() ?? "EmailOrPasswordCannotBeEmpty"}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        try
                        {
                            _accountRepository.Create(new Account(Email!, Password!));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        wnd.Close();
                    });
                    break;
                default:
                    throw new Exception("Illegal mode!");
            }
        }
    }
}
