using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;
using MessageBox = System.Windows.MessageBox;

namespace TagReporter.ViewModels;


public class SettingsViewModel : ObservableRecipient
{
    private string? _savePath;
    private string? _selectedLanguage;
    private readonly ConfigRepository _configRepository;
    public ResourceDictionaryService ResourceDictionaryService { get; }

    public Dictionary<string, string> Languages => ResourceDictionaryService.AvailableLanguages;

    public string? SavePath
    {
        get => _savePath;
        set => SetProperty(ref _savePath, value);
    }

    public string? SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public ICommand StoreSavePathCmd { get; }
    public ICommand LangSelectionChangedCmd { get; }
    public ICommand SelectDirCmd { get; }

    public SettingsViewModel(ConfigRepository configRepository, ResourceDictionaryService resourceDictionaryService)
    {
        _configRepository = configRepository;
        ResourceDictionaryService = resourceDictionaryService;
        var savePathConfigParam = _configRepository.FindAll().Find(x => x.Parameter == "save_path");
        SavePath = savePathConfigParam != null ? savePathConfigParam.Value : @".";
        SelectedLanguage = _configRepository.Collection().FindById("language").Value;

        LangSelectionChangedCmd = new RelayCommand(() =>
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedLanguage!);
            _configRepository.Update(_configRepository.Collection().FindById("language"), new Config { Parameter = "language", Value = SelectedLanguage! });
            ResourceDictionaryService.CurrentLanguage = SelectedLanguage;
        });

        StoreSavePathCmd = new RelayCommand(StoreSavePath);

        SelectDirCmd = new RelayCommand(() =>
        {
            FolderBrowserDialog dialog = new();
            dialog.SelectedPath = SavePath;
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                SavePath = ".";
            }

            SavePath = dialog.SelectedPath;
            StoreSavePath();
        });
    }

    public void StoreSavePath()
    {
        var savePathConfigParameter = _configRepository.FindAll().Find(x => x.Parameter == "save_path");
        if (!Directory.Exists(SavePath))
        {
            MessageBox.Show($"Directory {SavePath} is not exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SavePath = ".";
            return;
        }

        if (savePathConfigParameter != null)
        {
            _configRepository.Update(savePathConfigParameter, new Config
            {
                Value = SavePath
            });
        }
        else
        {
            _configRepository.Create(new Config
            {
                Parameter = "save_path",
                Value = SavePath
            });
        }
    }
}