using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class ZoneMgrViewModel : ReactiveObject
    {
        private readonly ZoneRepository _zoneRepository = new();

        public ObservableCollection<Zone> Zones { get; } = new();

        private Zone? _selectedItem;
        public Zone? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;

                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                this.RaisePropertyChanged(nameof(IsSelected));
            }
        }
        public bool IsSelected { get => _selectedItem != null; }

        public ReactiveCommand<Unit, Unit>? AddCmd { get; }
        public ReactiveCommand<Unit, Unit>? EditCmd { get; }
        public ReactiveCommand<Unit, Unit>? RemoveCmd { get; }


        public void UpdateZones()
        {
            Zones.Clear();
            _zoneRepository.FindAll().ToList().ForEach(Zones.Add);
        }

        public ZoneMgrViewModel()
        {
            UpdateZones();
            AddCmd = ReactiveCommand.Create(() => {
                ShowEditDialog(EditMode.Create);

            });
            EditCmd = ReactiveCommand.Create(() => {
                ShowEditDialog(EditMode.Edit, SelectedItem);
            });
            RemoveCmd = ReactiveCommand.Create(() =>
            {
                if (SelectedItem != null)
                {
                    _zoneRepository.Delete(SelectedItem);
                    UpdateZones();
                }
            });
        }

        public void ShowEditDialog(EditMode mode, Zone? zone = null)
        {
            var wnd = new ZoneEditWindow(mode, zone);
            wnd.GetResourceDictionary().MergedDictionaries.Clear();
            wnd.GetResourceDictionary().MergedDictionaries.Add(CommonResources.GetLangResourceDictionary());
            wnd.ViewModel!.AddEditCmd.Subscribe(_ => {
                UpdateZones();
            });
            wnd.ShowDialog();
        }
    }
}
