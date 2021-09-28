using System.Reactive.Disposables;
using ReactiveUI;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for ReportFormPage.xaml
    /// </summary>
    public partial class ReportFormPage : IViewFor<ReportFormViewModel>, IViewComponent
    {       
        public ReportFormViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ReportFormViewModel?)value; }
        private IMediator? _mediator;

        public ReportFormPage()
        {
            InitializeComponent();
            ViewModel = new ReportFormViewModel();
            DataContext = ViewModel;
            Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.SetMorningTimeCmd);
        }

        public ResourceDictionary GetResourceDictionary() => PageResourceDictionary;
        

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }
        public void ZoneGroupListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
        {
            Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.ZoneGroupDblClickCmd);
        }
    }
}
