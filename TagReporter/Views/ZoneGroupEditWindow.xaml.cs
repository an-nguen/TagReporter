using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for ZoneGroupEditWindow.xaml
/// </summary>
public partial class ZoneGroupEditWindow
{
    public ZoneGroupEditWindow(ZoneGroupEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public ZoneGroupEditViewModel ViewModel => (ZoneGroupEditViewModel)DataContext;
}