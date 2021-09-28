using System;
using System.Collections.Generic;
using LiteDB;
using ReactiveUI;

namespace TagReporter.Models
{
    public class Zone: ReactiveObject
    {
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

        [BsonIgnore] public List<Tag> Tags { get; set; } = new List<Tag>();

        [BsonId(true)]
        public Guid Uuid { get; set; }
        public string? Name { get; set; }

    }
}
