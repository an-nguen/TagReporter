using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class ZoneMgrViewModel : ObservableRecipient
{
    private readonly IZoneRepository _zoneRepository;
    private readonly IZoneEditWindowFactory _zoneEditWindowFactory;
    public ResourceDictionaryService ResourceDictionaryService { get; }

    public ObservableCollection<Zone> Zones { get; } = new();

    private Zone? _selectedItem;
    private readonly StatusBarService _statusBarService;

    public Zone? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public bool IsSelected => SelectedItem != null;

    public ICommand AddCmd { get; }
    public ICommand EditCmd { get; }
    public ICommand RemoveCmd { get; }

    public async void UpdateZones()
    {
        Zones.Clear();
        _statusBarService.Loading = true;
        var zones = await _zoneRepository.FindAllAsync();
        zones.ForEach(Zones.Add);
        _statusBarService.Loading = false;
    }

    public ZoneMgrViewModel(IZoneRepository zoneRepository, 
        IZoneEditWindowFactory zoneEditWindowFactory,
        ResourceDictionaryService resourceDictionaryService,
        StatusBarService statusBarService)
    {
        _zoneRepository = zoneRepository;
        _zoneEditWindowFactory = zoneEditWindowFactory;
        ResourceDictionaryService = resourceDictionaryService;
        _statusBarService = statusBarService;
        UpdateZones();
        AddCmd = new RelayCommand(() =>
        {
            ShowEditDialog(EditMode.Create);

        });
        EditCmd = new RelayCommand(() =>
        {
            ShowEditDialog(EditMode.Edit, SelectedItem);
        });
        RemoveCmd = new RelayCommand(() =>
        {
            if (SelectedItem == null) return;
            _zoneRepository.Delete(SelectedItem.Id);
            UpdateZones();
        });
    }

    public void ShowEditDialog(EditMode mode, Zone? zone = null)
    {
        if (zone != null)
            zone.TagUuids = _zoneRepository.FindTagUuidsByZone(zone);
        var window = _zoneEditWindowFactory.Create(mode, zone);
        window.ViewModel.CloseCmd = new RelayCommand(() =>
        {
            UpdateZones();
            window.Close();
        });
        window.ShowDialog();
    }
}