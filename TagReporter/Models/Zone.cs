using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactiveUI;

namespace TagReporter.Models;

public class Zone: ReactiveObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    [NotMapped] public List<Tag> Tags { get; set; } = new List<Tag>();

    [NotMapped]
    private bool _isChecked;

    [NotMapped]
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    [NotMapped]
    public List<Guid> TagUuids { get; set; } = new();
}
