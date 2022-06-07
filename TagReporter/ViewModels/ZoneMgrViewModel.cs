using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

    public async Task UpdateZones()
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
        AddCmd = new RelayCommand(() =>
        {
            ShowEditDialog(DialogMode.Create);

        });
        EditCmd = new RelayCommand(() =>
        {
            ShowEditDialog(DialogMode.Edit, SelectedItem);
        });
        RemoveCmd = new RelayCommand(async () =>
        {
            if (SelectedItem == null) return;
            await _zoneRepository.Delete(SelectedItem.Id);
            await UpdateZones();
        });
        UpdateZones();
    }

    public void ShowEditDialog(DialogMode mode, Zone? zone = null)
    {
        if (zone != null)
            zone.TagUuids = _zoneRepository.FindTagUuidsByZone(zone);
        var window = _zoneEditWindowFactory.Create(mode, zone);
        window.ViewModel.CloseCmd = new RelayCommand(async () =>
        {
            await UpdateZones();
            window.Close();
        });
        window.ShowDialog();
    }
}