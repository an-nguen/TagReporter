using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class AccountMgrViewModel: ReactiveObject
    {
        private AccountRepository _accountRepository { get; } = new();

        public ObservableCollection<Account> Accounts { get; } = new();

        private Account? _selectedAccount;
        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                
                this.RaiseAndSetIfChanged(ref _selectedAccount, value);
                this.RaisePropertyChanged(nameof(IsSelected));
            }
        }
        public bool IsSelected { get => _selectedAccount != null;  }

        public ReactiveCommand<Unit, Unit>? AddCmd { get; }
        public ReactiveCommand<Unit, Unit>? EditCmd { get; }
        public ReactiveCommand<Unit, Unit>? RemoveCmd { get; }

        private void UpdateAccounts()
        {
            Accounts.Clear();
            _accountRepository.FindAll().ToList().ForEach(Accounts.Add);

        }

        public AccountMgrViewModel()
        {
            UpdateAccounts();
            AddCmd = ReactiveCommand.Create(() => {
                ShowEditDialog(EditMode.Create);
            });
            EditCmd = ReactiveCommand.Create(() => {
                ShowEditDialog(EditMode.Edit, SelectedAccount);
            });
            RemoveCmd = ReactiveCommand.Create(() =>
            {
                if (SelectedAccount != null)
                {
                    _accountRepository.Delete(SelectedAccount);
                    var account = Accounts.First(a => a.Email == SelectedAccount.Email);
                    Accounts.Remove(account);
                }
            });
            
        }

        public void ShowEditDialog(EditMode mode, Account? account = null)
        {
            var wnd = new AccountEditWindow(mode, account);
            wnd.GetResourceDictionary().MergedDictionaries.Clear();
            wnd.GetResourceDictionary().MergedDictionaries.Add(CommonResources.GetLangResourceDictionary());
            wnd.ViewModel!.AddEditCommand.Subscribe(_ => {
                UpdateAccounts();
            });
            wnd.ShowDialog();
        }
    }
}
