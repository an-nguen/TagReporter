using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Views;

namespace TagReporter.ViewModels
{
    public class ZoneEditViewModel: ReactiveObject
    {
        private ZoneRepository _zoneRepository { get; } = new();
        private ZoneTagRepository _zoneTagRepository { get; } = new();
        private TagRepository _tagRepository { get; } = new();

        public string Title { get; set; } = String.Empty;
        public string AddEditBtnContent { get; set; } = String.Empty;
        public string? Name { get; set; }
        private Zone _zone;

        public ObservableCollection<Tag> Tags { get; } = new();

        public ReactiveCommand<ZoneEditWindow, Unit> AddEditCmd { get; }
        public ReactiveCommand<ZoneEditWindow, Unit> CloseCmd { get; }

        public ReactiveCommand<Unit, Unit> SelectAllCmd { get; }
        public ReactiveCommand<Unit, Unit> DeselectAllCmd { get; }

        public ZoneEditViewModel(EditMode mode, Zone? zone)
        {
            var resource = CommonResources.GetLangResourceDictionary();

            _tagRepository.FindAll().ForEach(Tags.Add);
            _zone = zone ?? new();
            Name = _zone.Name;

            SelectAllCmd = ReactiveCommand.Create(() =>
            {
                foreach (var tag in Tags)
                {
                    tag.IsChecked = true;
                }  
            });
            DeselectAllCmd = ReactiveCommand.Create(() =>
            {
                foreach (var tag in Tags)
                {
                    tag.IsChecked = false;
                }
            });
            CloseCmd = ReactiveCommand.Create<ZoneEditWindow>((wnd) => wnd.Close());

            switch (mode)
            {
                case EditMode.Edit:
                    Title = AddEditBtnContent = resource["Edit"].ToString() ?? "Edit";
                    var tags = _tagRepository.FindTagsByZone(_zone);
                    foreach (var tag in tags)
                    {
                        var found = Tags.First(t => t.Uuid == tag.Uuid);
                        found.IsChecked = true;
                    }
                    AddEditCmd = ReactiveCommand.Create<ZoneEditWindow>((wnd) =>
                    {
                        try
                        {
                            _zoneRepository.Update(_zone, new()
                            {
                                Name = Name
                            });
                            var zoneTagList = _zoneTagRepository.Query().Where(zt => zt.ZoneUuid == _zone.Uuid).ToList();
                            zoneTagList.ForEach(zt => _zoneTagRepository.Delete(zt));
                            Tags.Where(t => t.IsChecked).ToList().ForEach(t =>
                            {
                                if (t.IsChecked)
                                {
                                    _zoneTagRepository.Create(new ZoneTag()
                                    {
                                        TagUuid = t.Uuid,
                                        ZoneUuid = _zone.Uuid
                                    });
                                }
                            });
                        } catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        wnd.Close();
                    });
                    break;
                case EditMode.Create:
                    Title = AddEditBtnContent = resource["Add"].ToString() ?? "Add";
                    AddEditCmd = ReactiveCommand.Create<ZoneEditWindow>((wnd) =>
                    {
                        try
                        {
                            var uuid = _zoneRepository.Create(new Zone()
                            {
                                Name = Name
                            }).AsGuid;
                            Tags.Where(t => t.IsChecked).ToList().ForEach(t =>
                            {
                                if (t.IsChecked)
                                {
                                    _zoneTagRepository.Create(new ZoneTag()
                                    {
                                        TagUuid = t.Uuid,
                                        ZoneUuid = uuid
                                    });
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
