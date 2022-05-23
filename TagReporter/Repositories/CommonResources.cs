using System;
using System.Threading;
using System.Windows;

namespace TagReporter.Repositories;

public static class CommonResources
{
    public static readonly Uri BaseAddress = new Uri("https://wirelesstag.net");
    public static readonly string ConnectionString = @"Filename=.\tag_reporter.db; Connection=Shared";

    public static ResourceDictionary GetLangResourceDictionary()
    {
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        var path = $"Views/Resources/lang.{currentCulture}.xaml";

        if (Uri.IsWellFormedUriString(path, UriKind.Relative))
        {
            return (ResourceDictionary)Application.LoadComponent(new Uri(path, UriKind.Relative));
        }

        return (ResourceDictionary)Application.LoadComponent(new Uri("Views/Resources/lang.en-US.xaml", UriKind.Relative));
    }
}