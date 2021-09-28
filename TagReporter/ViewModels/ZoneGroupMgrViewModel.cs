using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class ZoneGroupMgrViewModel : ReactiveObject
    {
        private readonly ZoneGroupRepository _zoneGroupRepository = new();

        public ObservableCollection<ZoneGroup> ZoneGroups { get; } = new();

        private ZoneGroup? _selectedItem;
        public ZoneGroup? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;

                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                this.RaisePropertyChanged(nameof(IsSelected));
            }
        }
        public bool IsSelected => _selectedItem != null;

        public ReactiveCommand<Unit, Unit> AddCmd { get; }
        public ReactiveCommand<Unit, Unit> EditCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveCmd { get; }

        public ZoneGroupMgrViewModel()
        {
            UpdateGroups();
            AddCmd = ReactiveCommand.Create(() =>
            {
                ShowEditDialog(EditMode.Create);
            });
            EditCmd = ReactiveCommand.Create(() =>
            {
                ShowEditDialog(EditMode.Edit, SelectedItem);
            });
            RemoveCmd = ReactiveCommand.Create(() =>
            {
                if (SelectedItem != null)
                {
                    _zoneGroupRepository.Delete(SelectedItem);
                    UpdateGroups();
                }
            });
        }

        public void UpdateGroups()
        {
            ZoneGroups.Clear();
            _zoneGroupRepository.FindAll().ForEach(ZoneGroups.Add);
        }

        public void ShowEditDialog(EditMode mode, ZoneGroup? group = null)
        {
            var wnd = new ZoneGroupEditWindow(mode, group);
            wnd.GetResourceDictionary().MergedDictionaries.Clear();
            wnd.GetResourceDictionary().MergedDictionaries.Add(CommonResources.GetLangResourceDictionary());
            wnd.ViewModel!.AddEditCmd.Subscribe(_ => {
                UpdateGroups();
            });
            wnd.ShowDialog();
        }
    }
}
