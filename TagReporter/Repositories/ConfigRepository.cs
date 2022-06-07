using LiteDB;
using TagReporter.Models;

namespace TagReporter.Repositories;

public class ConfigRepository : LiteDbBaseRepository<Config>
{
    public ConfigRepository() : base("config", CommonResources.ConnectionString)
    {
    }

    public new bool Update(Config objDb, Config obj)
    {
        using var db = new LiteDatabase(ConnString);
        var collection = db.GetCollection<Config>(TableName);
        objDb.Value = obj.Value;
        return collection.Update(objDb);
    }

    public new bool Delete(Config obj)
    {
        using var db = new LiteDatabase(ConnString);
        var collection = db.GetCollection<Config>(TableName);
        return collection.Delete(obj.Parameter);
    }
}