using System.Collections.Generic;
using LiteDB;

namespace TagReporter.Models;

public class ZoneGroup
{
    [BsonId(true)]
    public int Id { get; set; }
    public string? Name { get; set; }
    [BsonRef("zones")]
    public List<Zone> Zones { get; set; } = new();
}
