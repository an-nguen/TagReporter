
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for AccountEditWindow.xaml
/// </summary>
public partial class AccountEditWindow
{        

    public AccountEditWindow(AccountEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
    public AccountEditViewModel ViewModel => (AccountEditViewModel)DataContext;
}