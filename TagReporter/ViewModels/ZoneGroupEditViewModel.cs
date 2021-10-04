using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Windows;
using System.Windows.Input;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class ZoneGroupEditViewModel : ReactiveObject
    {
        private ZoneRepository _zoneRepository { get; } = new();
        private ZoneGroupRepository _zoneGroupRepository { get; } = new();

        public string? Title { get; set; }
        public string? AddEditBtnContent { get; set; }
        public string? Name { get; set; }
        private ZoneGroup _zoneGroup;

        public ObservableCollection<Zone> Zones { get; } = new();

        public ReactiveCommand<ZoneGroupEditWindow, Unit> AddEditCmd { get; }
        public ReactiveCommand<ZoneGroupEditWindow, Unit> CloseCmd { get; }

        public ReactiveCommand<Unit, Unit> SelectAllCmd { get; }
        public ReactiveCommand<Unit, Unit> DeselectAllCmd { get; }

        public ZoneGroupEditViewModel(EditMode mode, ZoneGroup? group)
        {
            var resource = CommonResources.GetLangResourceDictionary();

            _zoneRepository.FindAll().ForEach(Zones.Add);
            _zoneGroup = @group ?? new();
            Name = _zoneGroup.Name;

            SelectAllCmd = ReactiveCommand.Create(() =>
            {
                foreach (var z in Zones)
                {
                    z.IsChecked = true;
                }
            });
            DeselectAllCmd = ReactiveCommand.Create(() =>
            {
                foreach (var z in Zones)
                {
                    z.IsChecked = false;
                }
            });

            CloseCmd = ReactiveCommand.Create<ZoneGroupEditWindow>((wnd) => wnd.Close());

            switch (mode)
            {
                case EditMode.Edit:
                    Title = AddEditBtnContent = resource["Edit"].ToString() ?? "Edit";
                    foreach (var z in _zoneGroup.Zones)
                    {
                        var found = Zones.FirstOrDefault(t => t.Uuid == z.Uuid);
                        if (found != null) found.IsChecked = true;
                    }
                    AddEditCmd = ReactiveCommand.Create<ZoneGroupEditWindow>((wnd) =>
                    {
                        if (string.IsNullOrEmpty(Name))
                        {
                            MessageBox.Show($"{resource["NameEmptyError"].ToString() ?? "NameEmptyError"}!", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }
                        try
                        {
                            _zoneGroupRepository.Update(_zoneGroup, new ZoneGroup()
                            {
                                Name = Name,
                                Zones = Zones.Where(z => z.IsChecked).ToList()
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        wnd.Close();
                    });
                    break;
                case EditMode.Create:
                    Title = AddEditBtnContent = resource["Add"].ToString() ?? "Add";
                    AddEditCmd = ReactiveCommand.Create<ZoneGroupEditWindow>((wnd) =>
                    {
                        if (string.IsNullOrEmpty(Name))
                        {
                            MessageBox.Show($"{resource["NameEmptyError"].ToString() ?? "NameEmptyError"}!", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        try
                        {
                            _zoneGroupRepository.Create(new ZoneGroup()
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

                        wnd.Close();
                    });
                    break;
                default:
                    throw new Exception("Illegal mode!");
            }
        }
    }
}
