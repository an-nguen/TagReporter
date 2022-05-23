using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;
using TagReporter.Services;
using TagReporter.Views;

namespace TagReporter.ViewModels;

public class ZoneEditViewModel: ObservableRecipient
{
    private readonly IZoneRepository? _zoneRepository;
    private readonly ITagRepository? _tagRepository;
    public ResourceDictionaryService? ResourceDictionaryService { get; }

    public string Title { get; set; } = string.Empty;
    public string? AddEditBtnContent { get; set; }
    public string? Name { get; set; }
    private Zone? _zone;

    public ObservableCollection<Tag> Tags { get; } = new();

    public ICommand? AddEditCmd { get; set; }
    public ICommand? CloseCmd { get; set; }

    public ICommand SelectAllCmd { get; }
    public ICommand DeselectAllCmd { get; }

    public ZoneEditViewModel(IServiceProvider serviceProvider)
    {
        if (serviceProvider.GetService(typeof(IZoneRepository)) is IZoneRepository zoneRepository)
            _zoneRepository = zoneRepository;

        if (serviceProvider.GetService(typeof(ITagRepository)) is ITagRepository tagRepository)
            _tagRepository = tagRepository;

        if (serviceProvider.GetService(typeof(ResourceDictionaryService)) is ResourceDictionaryService
            resourceDictionaryService)
            ResourceDictionaryService = resourceDictionaryService;


        SelectAllCmd = new RelayCommand(() =>
        {
            foreach (var tag in Tags)
                tag.IsChecked = true;
        });
        DeselectAllCmd = new RelayCommand(() =>
        {
            foreach (var tag in Tags)
                tag.IsChecked = false;
        });
        UpdateTags();
    }

    public void SetMode(EditMode mode, Zone? zone)
    {
        _zone = zone;
        Name = _zone?.Name;

        switch (mode)
        {
            case EditMode.Edit:
                Title = AddEditBtnContent = ResourceDictionaryService?["Edit"] ?? "Edit";
                foreach (var found in Tags.Where(t => _zone!.TagUuids.Contains(t.Uuid)))
                    found.IsChecked = true;

                AddEditCmd = new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(Name))
                    {
                        MessageBox.Show($"{ResourceDictionaryService?["NameCannotBeEmpty"] ?? "NameCannotBeEmpty"}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        _zoneRepository?.Update(_zone!.Id, new Zone
                        {
                            Name = Name,
                            TagUuids = Tags.Where(t => t.IsChecked).Select(t => t.Uuid).ToList()
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
                        MessageBox.Show($"{ResourceDictionaryService?["NameCannotBeEmpty"] ?? "NameCannotBeEmpty"}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        _zoneRepository?.Create(new Zone
                        {
                            Name = Name,
                            TagUuids = Tags.Where(t => t.IsChecked).Select(t => t.Uuid).ToList(),
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    CloseCmd?.Execute(null);
                });
                break;
            default:
                throw new Exception("Illegal mode!");
        }

    }

    private void UpdateTags()
    {
        var tags = _tagRepository?.FindAll()!;
        tags.ForEach(Tags.Add);
    }

}