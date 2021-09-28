using Newtonsoft.Json;
using OfficeOpenXml;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagReporter.DTOs;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class MainViewModel : ReactiveObject, INotifyPropertyChanged, IMediator
    {
        public NavigationService NavigationService { get; } = NavigationService.Instance;
        public StatusBarResource StatusBarResource { get; } = StatusBarResource.Instance;

        private AccountRepository _accountRepository { get; } = new();
        private ZoneTagRepository _zoneTagRepository = new();
        private TagRepository _tagRepository { get; } = new();
        private ConfigRepository _configRepository { get; } = new();

        private List<IViewComponent> _viewComponents = new();

        private ReportFormPage? _reportFormPage;
        public ReportFormPage? ReportFormPage
        {
            get
            {
                if (_reportFormPage == null)
                {
                    _reportFormPage = new ReportFormPage();
                    RegisterView(_reportFormPage);
                }
                return _reportFormPage;
            }
        }
        private AccountManagerPage? _accountManagerPage;
        public AccountManagerPage? AccountManagerPage
        {
            get
            {
                if (_accountManagerPage == null)
                {
                    _accountManagerPage = new AccountManagerPage();
                    RegisterView(_accountManagerPage);
                }
                return _accountManagerPage;
            }
        }

        private ZoneManagerPage? _zoneManagerPage;
        public ZoneManagerPage? ZoneManagerPage
        {
            get
            {
                if (_zoneManagerPage == null)
                {
                    _zoneManagerPage = new ZoneManagerPage();
                    RegisterView(_zoneManagerPage);
                }
                return _zoneManagerPage;
            }
        }

        private ZoneGroupManagerPage? _zoneGroupManagerPage;
        public ZoneGroupManagerPage? ZoneGroupManagerPage
        {
            get
            {
                if (_zoneGroupManagerPage == null)
                {
                    _zoneGroupManagerPage = new ZoneGroupManagerPage();
                    RegisterView(_zoneGroupManagerPage);
                }
                return _zoneGroupManagerPage;
            }
        }

        private SettingsPage? _settingsPage;
        public SettingsPage? SettingsPage
        {
            get
            {
                if (_settingsPage == null)
                {
                    _settingsPage = new SettingsPage();
                    RegisterView(_settingsPage);
                }
                return _settingsPage;
            }
        }

        public ReactiveCommand<Unit, Unit> OpenReportFormPageCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenAccountMgrPageCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenZoneMgrPageCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenZoneGroupMgrPageCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsPageCmd { get; }
        public ReactiveCommand<Unit, Unit> GoBackCmd { get; }
        public ReactiveCommand<Unit, Unit> GoForwardCmd { get; }
        public ReactiveCommand<Unit, Unit> SyncTagsFromCloudCmd { get; }

        private ResourceDictionary _resourceDictionary;

        public MainViewModel(Frame contentFrame, ResourceDictionary resourceDictionary)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            NavigationService.Frame = contentFrame;
            _resourceDictionary = resourceDictionary;

            // Set language
            var languageConfig = _configRepository.Collection().FindById("language");
            if (languageConfig == null)
            {
                languageConfig = new Config()
                {
                    Parameter = "language",
                    Value = "en-US"
                };
                _configRepository.Create(languageConfig);
            }
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(languageConfig.Value!);
            UpdateMainWindowLanguage();

            OpenReportFormPageCommand = ReactiveCommand.Create(() => NavigationService.NavigateTo(ReportFormPage!));
            OpenAccountMgrPageCommand = ReactiveCommand.Create(() => NavigationService.NavigateTo(AccountManagerPage!));
            OpenZoneMgrPageCommand = ReactiveCommand.Create(() => NavigationService.NavigateTo(ZoneManagerPage!));
            OpenZoneGroupMgrPageCommand = ReactiveCommand.Create(() => NavigationService.NavigateTo(ZoneGroupManagerPage!));
            OpenSettingsPageCmd = ReactiveCommand.Create(() =>
            {
                var settingsPage = SettingsPage;
                settingsPage!.ViewModel!.LangSelectionChangedCmd.Subscribe(_ =>
                {
                    UpdateMainWindowLanguage();
                    ReloadResourceDictionary();
                });
                NavigationService.NavigateTo(settingsPage!);
            });

            SyncTagsFromCloudCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                var resource = CommonResources.GetLangResourceDictionary();
                StatusBarResource.StatusBarLoading = true;
                StatusBarResource.StatusBarStringContent = resource["LoadingTags"].ToString();

                var accounts = _accountRepository.FindAll();
                await LoadTagsRemotely(accounts);

                StatusBarResource.StatusBarStringContent = resource["TagsLoaded"].ToString();
                StatusBarResource.StatusBarLoading = false;
            });

            GoBackCmd = ReactiveCommand.Create(() => NavigationService.GoBack());
            GoForwardCmd = ReactiveCommand.Create(() => NavigationService.GoForward());
        }

        public void UpdateMainWindowLanguage()
        {
            _resourceDictionary.MergedDictionaries.Clear();
            _resourceDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Views/Resources/style.xaml", UriKind.Relative) });
            _resourceDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Views/Resources/lang.{Thread.CurrentThread.CurrentCulture}.xaml", UriKind.Relative) });
        }

        public async Task<bool> LoadTagsRemotely(List<Account> accounts)
        {
            var resource = CommonResources.GetLangResourceDictionary();

            // Remove all tags from database
            _tagRepository.RemoveAll();
            try
            {
                foreach (var account in accounts)
                {
                    if (!await account.SignIn(CommonResources.BaseAddress))
                    {
                        MessageBox.Show($"{resource["FetchTagsFailed"].ToString() ?? "FetchTagsFailed"} {account.Email}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }
                    GetTagsRemotely(account);
                }
                // Remove relationships Zone-Tag where tag is removed

                var zoneTags = _zoneTagRepository.FindAll();
                zoneTags.ForEach(zt =>
                {
                    var found = _tagRepository.FindTagsById(zt.TagUuid);
                    if (found == null)
                    {
                        _zoneTagRepository.Delete(zt);
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
        private void GetTagsRemotely(Account account)
        {
            // Fetch tag list from cloud
            var cookieContainer = new CookieContainer();
            using var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };
            using var client = new HttpClient(handler) { BaseAddress = CommonResources.BaseAddress };
            cookieContainer.Add(CommonResources.BaseAddress, new Cookie("WTAG", account.SessionId));
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var result = client.PostAsync("/ethClient.asmx/GetTagList2", content).Result;
            string responseBody = result.Content.ReadAsStringAsync().Result;

            var jsonResponse =
                JsonConvert.DeserializeObject<DefaultWstResponse<TagResponse>>(responseBody);
            if (jsonResponse?.d == null || jsonResponse.d.Count == 0)
            {
                return;
            }

            foreach (var tagResponse in jsonResponse.d)
            {
                var tag = new Tag(Guid.Parse(tagResponse.uuid!), tagResponse.name!)
                {
                    TagManagerName = tagResponse.managerName,
                    TagManagerMAC = tagResponse.mac,
                    Account = account
                };
                var dbTag = _tagRepository.FindAll().FirstOrDefault(t => t.Uuid == tag.Uuid);
                if (dbTag != null)
                {
                    continue;
                }
                _tagRepository.Create(tag);
            }
        }


        public new event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RegisterView(IViewComponent o)
        {
            o.SetMediator(this);
            _viewComponents.Add(o);
            ReloadResourceDictionary();
        }

        public void ReloadResourceDictionary()
        {
            _viewComponents.ForEach(vc =>
            {
                vc.GetResourceDictionary().Clear();
                vc.GetResourceDictionary().MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("Views/Resources/style.xaml", UriKind.Relative) });
                vc.GetResourceDictionary().MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Views/Resources/lang.{Thread.CurrentThread.CurrentCulture}.xaml", UriKind.Relative) });
            });
        }
    }
}
