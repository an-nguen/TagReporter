using LiteDB;

namespace TagReporter.Models;

public class Config
{
    [BsonId(false)]
    public string? Parameter { get; set; }
    public string? Value { get; set; }
}
