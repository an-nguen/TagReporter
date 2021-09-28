using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TagReporter.Models
{
    public class ZoneGroup
    {
        [BsonId(true)]
        public int Id { get; set; }
        public string? Name { get; set; }
        [BsonRef("zones")]
        public List<Zone> Zones { get; set; } = new();
    }
}
