﻿using ReactiveUI;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ZoneGroupEditWindow.xaml
    /// </summary>
    public partial class ZoneGroupEditWindow : IViewFor<ZoneGroupEditViewModel>, IViewComponent
    {        
        public ZoneGroupEditViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ZoneGroupEditViewModel?)value; }
        private IMediator? _mediator;

        private ZoneGroupEditWindow() { }
        public ZoneGroupEditWindow(EditMode mode, ZoneGroup? zoneGroup = null)
        {
            ViewModel = new ZoneGroupEditViewModel(mode, zoneGroup);
            InitializeComponent();
            DataContext = ViewModel;

            this.WhenActivated(d =>
            {
                this.AddEditBtn.Events().PreviewMouseLeftButtonUp.Select(_ => this)
                    .InvokeCommand(ViewModel, vm => vm.AddEditCmd)
                    .DisposeWith(d);
                this.CloseBtn.Events().PreviewMouseLeftButtonUp.Select(_ => this)
                    .InvokeCommand(ViewModel, vm => vm.CloseCmd)
                    .DisposeWith(d);


                this.Events().KeyDown.Where(x => x.Key == Key.Enter)
                    .Select(_ => this)
                    .InvokeCommand(this, v => v.ViewModel!.AddEditCmd)
                    .DisposeWith(d);
                this.Events().KeyDown.Where(keyEventArgs => keyEventArgs.Key == Key.Escape)
                    .Select(_ => this)
                    .InvokeCommand(ViewModel, v => v.CloseCmd)
                    .DisposeWith(d);
            });
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ResourceDictionary GetResourceDictionary() => WindowResourceDictionary;
    }
}
