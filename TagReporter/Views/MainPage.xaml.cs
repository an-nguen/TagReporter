using System.Windows.Controls;
using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for MainPage.xaml
/// </summary>
public partial class MainPage: Page
{       
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.SetMorningTimeCmd.Execute(null);
    }


    public void ZoneGroupListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
    {
        ViewModel.ZoneGroupDblClickCmd.Execute(null);
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;
}