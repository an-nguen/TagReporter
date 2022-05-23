using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using TagReporter.Contracts.Repositories;
using TagReporter.Contracts.Services;
using TagReporter.Repositories;
using TagReporter.Services;
using TagReporter.ViewModels;
using TagReporter.Views;

namespace TagReporter;

public partial class App
{
    private IHost? _host;

    public App()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        var appLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);

        _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureAppConfiguration(c => c.SetBasePath(appLocation))
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                services.AddDbContext<AppContext>(options =>
                {
                    options.UseSqlServer(context.Configuration.GetConnectionString("TagReporterDatabase"));
                });

                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IAccountRepository, AccountRepository>();
                services.AddSingleton<ITagRepository, TagRepository>();
                services.AddSingleton<IZoneRepository, ZoneRepository>();
                services.AddSingleton<ResourceDictionaryService>();
                services.AddSingleton<StatusBarService>();
                services.AddSingleton<ReportService>();

                services.AddSingleton<ZoneGroupRepository>();
                services.AddSingleton<ConfigRepository>();

                services.AddTransient<ZoneMgrViewModel>();
                services.AddTransient<ZoneManagerPage>();

                services.AddTransient<AccountMgrViewModel>();
                services.AddTransient<AccountManagerPage>();

                services.AddTransient<ZoneGroupMgrViewModel>();
                services.AddTransient<ZoneGroupManagerPage>();

                services.AddTransient<ShellViewModel>();
                services.AddTransient<ShellWindow>();

                services.AddTransient<MainViewModel>();
                services.AddTransient<MainPage>();

                services.AddTransient<SettingsViewModel>();
                services.AddTransient<SettingsPage>();

                services.AddTransient<IAccountEditWindowFactory, AccountEditWindowFactory>();
                services.AddTransient<IZoneEditWindowFactory, ZoneEditWindowFactory>();
                services.AddTransient<IZoneGroupEditWindowFactory, ZoneGroupEditWindowFactory>();

                services.AddTransient<AccountEditViewModel>();
                services.AddTransient<ZoneEditViewModel>();
                services.AddTransient<ZoneGroupEditViewModel>();
            }).Build();
        await _host.RunAsync();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host!.StopAsync();
    }
}