using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class ZoneGroupMgrViewModel : ObservableRecipient
{
    private readonly ZoneGroupRepository _zoneGroupRepository;
    private readonly IZoneGroupEditWindowFactory _zoneGroupEditWindowFactory;
    public ResourceDictionaryService ResourceDictionaryService { get; }

    public ObservableCollection<ZoneGroup> ZoneGroups { get; } = new();

    private ZoneGroup? _selectedItem;
    public ZoneGroup? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public bool IsSelected => _selectedItem != null;

    public ICommand AddCmd { get; }
    public ICommand EditCmd { get; }
    public ICommand RemoveCmd { get; }

    public ZoneGroupMgrViewModel(ZoneGroupRepository zoneGroupRepository, 
        IZoneGroupEditWindowFactory zoneGroupEditWindowFactory, 
        ResourceDictionaryService resourceDictionaryService)
    {
        _zoneGroupRepository = zoneGroupRepository;
        _zoneGroupEditWindowFactory = zoneGroupEditWindowFactory;
        ResourceDictionaryService = resourceDictionaryService;
        UpdateGroups();
        AddCmd = new RelayCommand(() =>
        {
            ShowEditDialog(DialogMode.Create);
        });
        EditCmd = new RelayCommand(() =>
        {
            ShowEditDialog(DialogMode.Edit, SelectedItem);
        });
        RemoveCmd = new RelayCommand(() =>
        {
            if (SelectedItem == null) return;
            _zoneGroupRepository.Delete(SelectedItem);
            UpdateGroups();
        });
    }

    public void UpdateGroups()
    {
        ZoneGroups.Clear();
        _zoneGroupRepository.FindAll().ForEach(ZoneGroups.Add);
    }

    public void ShowEditDialog(DialogMode mode, ZoneGroup? group = null)
    {
        var wnd = _zoneGroupEditWindowFactory.Create(mode, group);
        wnd.ViewModel.CloseCmd = new RelayCommand(() =>
        {
            UpdateGroups();
            wnd.Close();
        });
        wnd.ShowDialog();
    }
}