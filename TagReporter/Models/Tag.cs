using System;
using System.Collections.Generic;
using LiteDB;
using ReactiveUI;

namespace TagReporter.Models
{
    public class Tag: ReactiveObject
    {
        public Tag() {}
        public Tag(Guid uuid, string name) : this()
        {
            Uuid = uuid;
            Name = name;
        }

        [BsonId]
        public Guid Uuid { get; set; }

        public string? Name { get; set; }

        public string? TagManagerName { get; set; }

        public string? TagManagerMAC { get; set; }

        private bool _isChecked;

        [BsonIgnore]
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref _isChecked, value);
            }
        }

        [BsonIgnore]
        public Account? Account { get; set; }

        [BsonIgnore]
        public List<Measurement> Measurements { get; set; } = new();
    }
}
