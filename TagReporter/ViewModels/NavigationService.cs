using System.Windows;
using System.Windows.Controls;

namespace TagReporter.ViewModels
{
    public class NavigationService
    {
        private static NavigationService? _instance;
        public Frame? Frame { get; set; } 

        private NavigationService() { }

        public static NavigationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NavigationService();
                }
                return _instance;
            }
        }


        public void NavigateTo(object content)
        {
            if (Frame == null)
            {
                MessageBox.Show("ContentFrame is null!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Frame.Navigate(content);
        }

        public void GoBack()
        {
            if (Frame == null)
            {
                MessageBox.Show("ContentFrame is null!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Frame.GoBack();
        }

        public void GoForward()
        {
            if (Frame == null)
            {
                MessageBox.Show("ContentFrame is null!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Frame.GoForward();
        }
    }
}
