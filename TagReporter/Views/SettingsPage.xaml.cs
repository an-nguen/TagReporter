using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, IViewFor<SettingsViewModel>, IViewComponent
    {
        private IMediator _mediator;

        public SettingsViewModel? ViewModel { get; set; }
        object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (SettingsViewModel?)value; }

        public SettingsPage()
        {
            InitializeComponent();
            ViewModel = new SettingsViewModel();
            DataContext = ViewModel;
        }


        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ResourceDictionary GetResourceDictionary() => PageResourceDictionary;
    }
}
