using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TagReporter.ViewModels;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for ZoneGroupManagerPage.xaml
    /// </summary>
    public partial class ZoneGroupManagerPage : IViewFor<ZoneGroupMgrViewModel>, IViewComponent
    {        
        public ZoneGroupMgrViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ZoneGroupMgrViewModel?)value; }
        private IMediator? _mediator;

        public ZoneGroupManagerPage()
        {
            InitializeComponent();
            ViewModel = new ZoneGroupMgrViewModel();
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
