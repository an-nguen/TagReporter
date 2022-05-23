using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for ZoneEditWindow.xaml
/// </summary>
public partial class ZoneEditWindow
{
    private ZoneEditWindow() { }
    public ZoneEditWindow(ZoneEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public ZoneEditViewModel ViewModel => (ZoneEditViewModel)DataContext;

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ViewModel.AddEditCmd?.Execute(null);
        if (e.Key == Key.Escape)
            ViewModel.CloseCmd?.Execute(null);
    }
}