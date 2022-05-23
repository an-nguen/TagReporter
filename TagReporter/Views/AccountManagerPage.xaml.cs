using ReactiveUI;
using System.Reactive.Linq;
using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for AccountManagerPage.xaml
/// </summary>
public partial class AccountManagerPage
{
    public AccountManagerPage(AccountMgrViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
    }

    public void ListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
    {
        Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.EditCmd);
    }

    public AccountMgrViewModel ViewModel => (AccountMgrViewModel)DataContext;
}

