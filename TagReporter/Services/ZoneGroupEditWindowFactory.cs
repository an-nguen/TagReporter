using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.ViewModels;
using TagReporter.Views;

namespace TagReporter.Services;

public class ZoneGroupEditWindowFactory: IZoneGroupEditWindowFactory
{
    private readonly ZoneGroupEditViewModel _viewModel;

    public ZoneGroupEditWindowFactory(ZoneGroupEditViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public ZoneGroupEditWindow Create(DialogMode mode, ZoneGroup? group)
    {
        var wnd = new ZoneGroupEditWindow(_viewModel);
        _viewModel.SetMode(mode, group);
        return wnd;
    }
}