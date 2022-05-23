using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for ZoneManagerPage.xaml
/// </summary>
public partial class ZoneManagerPage
{
    public ZoneManagerPage(ZoneMgrViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public ZoneMgrViewModel ViewModel => (ZoneMgrViewModel)DataContext;

    public void ListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
    {
        ViewModel.EditCmd.Execute(null);
    }
}