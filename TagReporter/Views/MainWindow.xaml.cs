using ReactiveUI;
using System.Reactive.Linq;
using TagReporter.ViewModels;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IViewFor<MainViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel(ContentFrame, MainWindowResourceDictionary);
            DataContext = ViewModel;
            Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.OpenReportFormPageCommand);
        }

        public MainViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainViewModel?)value; }
    }
}
