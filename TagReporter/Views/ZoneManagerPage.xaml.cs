using ReactiveUI;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagReporter.ViewModels;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for ZoneManagerPage.xaml
    /// </summary>
    public partial class ZoneManagerPage : IViewFor<ZoneMgrViewModel>, IViewComponent
    {
        public ZoneMgrViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ZoneMgrViewModel?)value; }
        private IMediator? _mediator;

        public ZoneManagerPage()
        {
            InitializeComponent();
            ViewModel = new ZoneMgrViewModel();
            DataContext = ViewModel;
        }


        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ResourceDictionary GetResourceDictionary() => PageResourceDictionary;
        public void ListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
        {
            Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.EditCmd);
        }
    }
}
