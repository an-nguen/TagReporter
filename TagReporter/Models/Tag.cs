using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactiveUI;

namespace TagReporter.Models;

public class Tag : ReactiveObject
{
    public Tag()
    {
    }

    public Tag(Guid uuid, string name) : this()
    {
        Uuid = uuid;
        Name = name;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Uuid { get; set; }

    public string? Name { get; set; }
    public string? TagManagerName { get; set; }
    public string? TagManagerMac { get; set; }

    [NotMapped] private bool _isChecked;

    [NotMapped]
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    [NotMapped] public WstAccount? Account { get; set; }

    [NotMapped] public List<Measurement> Measurements { get; set; } = new();
}