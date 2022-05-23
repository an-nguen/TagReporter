using TagReporter.ViewModels;

namespace TagReporter.Views;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}