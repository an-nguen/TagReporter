using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;
using TagReporter.Contracts.Services;
using TagReporter.ViewModels;
using TagReporter.Views;

namespace TagReporter.Services;

public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INavigationService _navigationService;
    private ShellWindow? _shellWindow;

    public ApplicationHostService(
        IServiceProvider serviceProvider,
        INavigationService navigationService)
    {
        _serviceProvider = serviceProvider;
        _navigationService = navigationService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (App.Current.Windows.OfType<ShellWindow>().Count() == 0)
        {
            // Default activation that navigates to the apps default page
            _shellWindow = _serviceProvider.GetService(typeof(ShellWindow)) as ShellWindow;
            if (_shellWindow == null) throw new Exception("ShellWindow not found in DI container");
            _navigationService.Initialize(_shellWindow.shellFrame);
            _shellWindow.Show();
            _navigationService.NavigateTo(typeof(MainViewModel).FullName!);
            await Task.CompletedTask;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}