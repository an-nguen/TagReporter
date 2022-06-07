using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.ApplicationServices;
using TagReporter.Contracts.Repositories;
using TagReporter.Contracts.Services;
using TagReporter.DTOs;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;

namespace TagReporter.ViewModels;

public class ShellViewModel : ObservableRecipient
{
    public INavigationService NavigationService { get; set; }
    public StatusBarService StatusBarService { get; }
    public ResourceDictionaryService ResourceDictionaryService { get; }

    private readonly IAccountRepository _accountRepository;
    private readonly ITagRepository _tagRepository;

    public string WindowTitle { get; set; }

    public ICommand OpenReportFormPageCommand { get; }
    public ICommand OpenAccountMgrPageCommand { get; }
    public ICommand OpenZoneMgrPageCommand { get; }
    public ICommand OpenZoneGroupMgrPageCommand { get; }
    public ICommand OpenSettingsPageCmd { get; }
    public ICommand GoBackCmd { get; }
    public ICommand GoForwardCmd { get; }
    public ICommand SyncTagsFromCloudCmd { get; }

    public bool CanGoBack => NavigationService.CanGoBack;
    public bool CanGoForward => NavigationService.CanGoForward;

    public ShellViewModel(INavigationService navigationService, IAccountRepository accountRepository,
        ITagRepository tagRepository, ResourceDictionaryService resourceDictionaryService,
        StatusBarService statusBarService)
    {
        NavigationService = navigationService;
        _accountRepository = accountRepository;
        _tagRepository = tagRepository;
        ResourceDictionaryService = resourceDictionaryService;
        StatusBarService = statusBarService;
        
        WindowTitle = $"Tag Reporter {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}";
        NavigationService.Navigated += (sender, s) =>
        {
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(CanGoBack));
        };

        OpenReportFormPageCommand = new RelayCommand(() => NavigationService.NavigateTo(typeof(MainViewModel).FullName!));
        OpenAccountMgrPageCommand = new RelayCommand(() => NavigationService.NavigateTo(typeof(AccountMgrViewModel).FullName!));
        OpenZoneMgrPageCommand = new RelayCommand(() => NavigationService.NavigateTo(typeof(ZoneMgrViewModel).FullName!));
        OpenZoneGroupMgrPageCommand = new RelayCommand(() => NavigationService.NavigateTo(typeof(ZoneGroupMgrViewModel).FullName!));
        OpenSettingsPageCmd = new RelayCommand(() =>
        {
            NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        });

        SyncTagsFromCloudCmd = new RelayCommand(async () =>
        {
            StatusBarService.Loading = true;
            StatusBarService.Content = ResourceDictionaryService["LoadingTags"];

            var accounts = _accountRepository.FindAll();
            await LoadTagsRemotely(accounts);

            StatusBarService.Content = ResourceDictionaryService["TagsLoaded"];
            StatusBarService.Loading = false;
        });

        GoBackCmd = new RelayCommand(() => NavigationService.GoBack());
        GoForwardCmd = new RelayCommand(() => NavigationService.GoForward());
    }

    public async Task<bool> LoadTagsRemotely(List<WstAccount> accounts)
    {
        // Remove all tags from database
        _tagRepository.RemoveAll();
        try
        {
            foreach (var account in accounts)
            {
                if (!await account.SignIn(CommonResources.BaseAddress))
                {
                    MessageBox.Show($"{ResourceDictionaryService["FetchTagsFailed"] ?? "FetchTagsFailed"} {account.Email}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    continue;
                }
                GetTagsRemotely(account);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show($"{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }
    private async void GetTagsRemotely(WstAccount wstAccount)
    {
        // Fetch tag list from cloud
        StatusBarService.Loading = true;
        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler()
        {
            CookieContainer = cookieContainer
        };
        using var client = new HttpClient(handler) { BaseAddress = CommonResources.BaseAddress };
        cookieContainer.Add(CommonResources.BaseAddress, new Cookie("WTAG", wstAccount.SessionId));
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var result = client.PostAsync("/ethClient.asmx/GetTagList2", content).Result;
        var responseBody = result.Content.ReadAsStringAsync().Result;

        var jsonResponse =
            JsonConvert.DeserializeObject<DefaultWstResponse<TagResponse>>(responseBody);
        if (jsonResponse?.D == null || jsonResponse.D.Count == 0)
        {
            return;
        }
        var dbTags = await _tagRepository.FindAllAsync();
        foreach (var tagResponse in jsonResponse.D)
        {
            var tag = new Tag(Guid.Parse(tagResponse.uuid!), tagResponse.name!)
            {
                TagManagerName = tagResponse.managerName,
                TagManagerMac = tagResponse.mac,
                Account = wstAccount
            };
            var dbTag = dbTags.FirstOrDefault(t => t.Uuid == tag.Uuid);
            if (dbTag != null)
                continue;
            await _tagRepository.Create(tag);
        }

        StatusBarService.Loading = false;
    }

    public static DateTime GetLinkerTime(Assembly? assembly)
    {
        const string buildVersionMetadataPrefix = "+build";

        if (assembly == null) return default;
        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion == null) return default;
        var value = attribute.InformationalVersion;
        var index = value.IndexOf(buildVersionMetadataPrefix, StringComparison.Ordinal);
        if (index <= 0) return default;
        value = value[(index + buildVersionMetadataPrefix.Length)..];
        return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);

    }
}