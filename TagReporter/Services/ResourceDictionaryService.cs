using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using TagReporter.Annotations;
using TagReporter.DTOs;
using TagReporter.Repositories;

namespace TagReporter.Services;

public class ResourceDictionaryService: INotifyPropertyChanged
{
    private ResourceDictionary? _resourceDictionary;
    private string? _currentLanguage;

    public Dictionary<string, string> AvailableLanguages = new()
    {
        {"en-US", "English"},
        {"ru-RU", "Russian"}
    };

    public string? CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            OnPropertyChanged(nameof(CurrentLanguage));
            OnPropertyChanged("Item[]");
        }
    }

    private void LoadDictionary()
    {
        var path = $"Views/Resources/lang.{CurrentLanguage}.xaml";

        if (Uri.IsWellFormedUriString(path, UriKind.Relative))
            _resourceDictionary = (ResourceDictionary)Application.LoadComponent(new Uri(path, UriKind.Relative));
        else
            _resourceDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("Views/Resources/lang.en-US.xaml", UriKind.Relative));
        
    }

    public string? this[string key] => _resourceDictionary?[key].ToString();


    public ResourceDictionaryService(ConfigRepository configRepository)
    {
        var langConfig = configRepository.FindAll().First(c => c.Parameter == "language");
        if (langConfig != null)
            CurrentLanguage = langConfig.Value;
        LoadDictionary();
    }

    public event PropertyChangedEventHandler? PropertyChanged;



    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        switch (propertyName)
        {
            case nameof(CurrentLanguage):
                LoadDictionary();
                break;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}