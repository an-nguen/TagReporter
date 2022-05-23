using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using TagReporter.Contracts.Services;
using TagReporter.Contracts.ViewModels;

namespace TagReporter.Services;

public class NavigationService: INavigationService 
{
    private readonly IPageService _pageService;
    private Frame? _frame;
    private object? _lastParameterUsed;

    public Frame Frame() => _frame;

    public event EventHandler<string>? Navigated;

    public bool CanGoBack => _frame?.CanGoBack ?? false;
    public bool CanGoForward => _frame?.CanGoForward ?? false;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void Initialize(Frame shellFrame)
    {
        if (_frame != null) return;
        _frame = shellFrame;
        _frame.Navigated += OnNavigated;
    }

    public void UnsubscribeNavigation()
    {
        _frame!.Navigated -= OnNavigated;
        _frame = null;
    }

    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            var vmBeforeNavigation = _frame.DataContext;
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
        }
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (_frame?.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            _frame!.Tag = clearNavigation;
            var page = _pageService.GetPage(pageKey);
            var navigated = _frame.Navigate(page, parameter);
            if (!navigated) return navigated;
            _lastParameterUsed = parameter;
            var dataContext = _frame.DataContext;
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return navigated;
        }

        return false;
    }

    private void OnNavigated(object? sender, NavigationEventArgs e)
    {
        if (sender is not Frame frame) return;

        var dataContext = frame.DataContext;
        if (dataContext is INavigationAware navigationAware)
            navigationAware.OnNavigatedTo(e.ExtraData);
        

        Navigated?.Invoke(sender, dataContext.GetType().FullName!);
    }

    public void GoForward()
    {
        _frame?.GoForward();
    }
}