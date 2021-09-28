using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
using System.Windows.Shapes;
using TagReporter.Models;
using TagReporter.ViewModels;

namespace TagReporter.Views
{
    /// <summary>
    /// Interaction logic for AccountEditWindow.xaml
    /// </summary>
    public partial class AccountEditWindow : IViewFor<AccountEditViewModel>, IViewComponent
    {        
        public AccountEditViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (AccountEditViewModel?)value; }

        private IMediator? _mediator;

        private AccountEditWindow() { }
        public AccountEditWindow(EditMode mode, Account? account = null)
        {
            ViewModel = new AccountEditViewModel(mode, account);
            InitializeComponent();
            DataContext = ViewModel;


            this.WhenActivated(disposable =>
            {
                this.AddEditBtn.Events().PreviewMouseLeftButtonUp.Select(_ => this)
                    .InvokeCommand(ViewModel, vm => vm.AddEditCommand)
                    .DisposeWith(disposable);
                this.CloseBtn.Events().PreviewMouseLeftButtonUp.Select(_ => this)
                    .InvokeCommand(ViewModel, vm => vm.CloseCommand)
                    .DisposeWith(disposable);


                this.Events().KeyDown.Where(x => x.Key == Key.Escape)
                    .Select(_ => this)
                    .InvokeCommand(this, v => v.ViewModel!.CloseCommand)
                    .DisposeWith(disposable);
                this.Events().KeyDown.Where(x => x.Key == Key.Enter)
                    .Select(_ => this)
                    .InvokeCommand(this, v => v.ViewModel!.AddEditCommand)
                    .DisposeWith(disposable);
            });
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ResourceDictionary GetResourceDictionary() => WindowResourceDictionary;
    }
}
