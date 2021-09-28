using System.ComponentModel;

namespace TagReporter
{
    public class StatusBarResource : INotifyPropertyChanged
    {
        private static StatusBarResource? _instance;

        private static string? _statusBarStringContent;
        public string? StatusBarStringContent
        {
            get => _statusBarStringContent;
            set
            {
                _statusBarStringContent = value;
                OnPropertyChanged(nameof(StatusBarStringContent));
            }
        }
        public bool _statusBarLoading;


        public bool StatusBarLoading
        {
            get => _statusBarLoading;
            set
            {
                _statusBarLoading = value;
                OnPropertyChanged(nameof(StatusBarLoading));
            }
        }

        private StatusBarResource() { }

        public static StatusBarResource Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StatusBarResource();
                }
                return _instance;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
