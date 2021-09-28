using System;
using LiteDB;

namespace TagReporter.Models
{
    public class ZoneTag
    {
        [BsonId(true)]
        public int Id { get; set; }

        public Guid ZoneUuid { get; set; }
        public Guid TagUuid { get; set; }
    }
}
