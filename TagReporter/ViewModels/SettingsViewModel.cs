using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using TagReporter.Models;
using TagReporter.Repositories;
using MessageBox = System.Windows.MessageBox;

namespace TagReporter.ViewModels
{
    public class Language
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Language(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
    public class SettingsViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private string? _savePath;
        private string? _selectedLanguage;
        private ConfigRepository _configRepository { get; } = new();

        public List<Language> Languages { get; } = new()
        {
            new Language("English", "en-US"),
            new Language("Russian", "ru-RU")
        };

        public string? SavePath
        {
            get => _savePath;
            set {
                _savePath = value;
                OnPropertyChanged(nameof(SavePath));
            }
        }

        public string? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        public ReactiveCommand<Unit, Unit> StoreSavePathCmd { get; }
        public ReactiveCommand<Unit, Unit> LangSelectionChangedCmd { get; }
        public ReactiveCommand<Unit, Unit> SelectDirCmd { get; }

        public SettingsViewModel()
        { 
            var savePathConfigParam = _configRepository.FindAll().Find(x => x.Parameter == "save_path");
            SavePath = (savePathConfigParam != null) ? savePathConfigParam.Value : @".";
            SelectedLanguage = _configRepository.Collection().FindById("language").Value;

            LangSelectionChangedCmd = ReactiveCommand.Create(() =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(SelectedLanguage!);
                _configRepository.Update(_configRepository.Collection().FindById("language"), new Config() { Parameter = "language", Value = SelectedLanguage! });
            }); 

            StoreSavePathCmd = ReactiveCommand.Create(StoreSavePath);

            SelectDirCmd = ReactiveCommand.Create(() =>
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
                _configRepository.Update(savePathConfigParameter, new Config()
                {
                    Value = SavePath
                });
            }
            else
            {
                _configRepository.Create(new Config()
                {
                    Parameter = "save_path",
                    Value = SavePath
                });
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
