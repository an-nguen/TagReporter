using System;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.ViewModels;
using TagReporter.Views;

namespace TagReporter.Services;

public class ZoneEditWindowFactory : IZoneEditWindowFactory
{
    private readonly ZoneEditViewModel _viewModel;

    public ZoneEditWindowFactory(ZoneEditViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public ZoneEditWindow Create(DialogMode mode, Zone? zone)
    {
        var wnd = new ZoneEditWindow(_viewModel);
        _viewModel.SetMode(mode, zone);
        return wnd;
    }
}