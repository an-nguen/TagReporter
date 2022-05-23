using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for ZoneGroupManagerPage.xaml
/// </summary>
public partial class ZoneGroupManagerPage
{        
    public ZoneGroupMgrViewModel ViewModel { get; }

    public ZoneGroupManagerPage(ZoneGroupMgrViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    public void ListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
    {
        ViewModel.EditCmd.Execute(null);
    }
}