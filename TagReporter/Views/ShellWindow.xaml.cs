using TagReporter.ViewModels;

namespace TagReporter.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}