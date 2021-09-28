using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AccountManagerPage.xaml
    /// </summary>
    public partial class AccountManagerPage : Page, IViewFor<AccountMgrViewModel>, IViewComponent
    {    
        public AccountMgrViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (AccountMgrViewModel?)value; }

        private IMediator? _mediator;

        public AccountManagerPage()
        {
            InitializeComponent();
            ViewModel = new AccountMgrViewModel();
            DataContext = ViewModel; 
        }

        public void ListViewItem_MouseDoubleClick(object o, MouseButtonEventArgs args)
        {
            Observable.Start(() => { }).InvokeCommand(ViewModel, vm => vm.EditCmd);
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ResourceDictionary GetResourceDictionary() => PageResourceDictionary;
    }
}
