using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class ZoneGroupEditViewModel : ObservableRecipient
{
    private readonly IZoneRepository? _zoneRepository;
    private readonly ZoneGroupRepository? _zoneGroupRepository;
    public ResourceDictionaryService ResourceDictionaryService { get; private set; }

    public string? Title { get; set; }
    public string? AddEditBtnContent { get; set; }
    public string? Name { get; set; }

    private ZoneGroup? _zoneGroup;

    public ObservableCollection<Zone> Zones { get; } = new();

    public ICommand? AddEditCmd { get; set; }
    public ICommand? CloseCmd { get; set; }

    public ICommand SelectAllCmd { get; }
    public ICommand DeselectAllCmd { get; }

    public ZoneGroupEditViewModel(IZoneRepository zoneRepository,
        ZoneGroupRepository zoneGroupRepository,
        ResourceDictionaryService resourceDictionaryService)
    {
        _zoneRepository = zoneRepository;
        _zoneGroupRepository = zoneGroupRepository;
        ResourceDictionaryService = resourceDictionaryService;
        UpdateZones();

        SelectAllCmd = new RelayCommand(() =>
        {
            foreach (var z in Zones)
                z.IsChecked = true;
        });
        DeselectAllCmd = new RelayCommand(() =>
        {
            foreach (var z in Zones)
                z.IsChecked = false;
        });
    }

    private async void UpdateZones()
    {
        var zones = await _zoneRepository?.FindAllAsync()!;
        zones.ForEach(Zones.Add);
    }

    public void SetMode(EditMode mode, ZoneGroup? group)
    {
        _zoneGroup = group ?? new ZoneGroup();
        Name = _zoneGroup.Name;



        switch (mode)
        {
            case EditMode.Edit:
                Title = AddEditBtnContent = ResourceDictionaryService?["Edit"] ?? "Edit";
                foreach (var z in _zoneGroup.Zones)
                {
                    var found = Zones.FirstOrDefault(t => t.Id == z.Id);
                    if (found != null) found.IsChecked = true;
                }
                AddEditCmd = new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        MessageBox.Show($"{ResourceDictionaryService?["NameEmptyError"] ?? "NameEmptyError"}!", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    try
                    {
                        _zoneGroupRepository?.Update(_zoneGroup, new ZoneGroup
                        {
                            Name = Name,
                            Zones = Zones.Where(z => z.IsChecked).ToList()
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    CloseCmd?.Execute(null);
                });
                break;
            case EditMode.Create:
                Title = AddEditBtnContent = ResourceDictionaryService?["Add"] ?? "Add";
                AddEditCmd = new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        MessageBox.Show($"{ResourceDictionaryService?["NameEmptyError"] ?? "NameEmptyError"}!", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        _zoneGroupRepository?.Create(new ZoneGroup()
                        {
                            Name = Name,
                            Zones = Zones.Where(z => z.IsChecked).ToList()
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    CloseCmd?.Execute(null);
                });
                break;
            default:
                throw new Exception("Illegal mode!");
        }
    }
}