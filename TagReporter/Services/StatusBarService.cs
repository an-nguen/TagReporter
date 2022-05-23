using System.ComponentModel;
using System.Runtime.CompilerServices;
using TagReporter.Annotations;
using TagReporter.Contracts.Services;

namespace TagReporter.Services;

public class StatusBarService : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set
        {
            _loading = value;
            _navigationService.Frame().IsEnabled = !_loading;
            OnPropertyChanged(nameof(Loading));
            OnPropertyChanged(nameof(IsNotLoading));
        }
    }

    public bool IsNotLoading => !Loading;

    private string? _content;

    public string? Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged(nameof(Content));
        }
    }

    public StatusBarService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}