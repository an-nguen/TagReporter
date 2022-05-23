using System.Collections.Generic;
using LiteDB;
using TagReporter.Models;

namespace TagReporter.Repositories;

public class ZoneGroupRepository: LiteDbBaseRepository<ZoneGroup>
{

    public ZoneGroupRepository() : base("zone_group", CommonResources.ConnectionString)
    {

    }

    public new List<ZoneGroup> FindAll()
    {
        using (var db = new LiteDatabase(ConnString))
        {
            return db.GetCollection<ZoneGroup>(TableName).Query().Include(x => x.Zones).ToList();
        }
    }

    public new bool Update(ZoneGroup objDb, ZoneGroup obj)
    {
        using (var db = new LiteDatabase(ConnString))
        {
            var collection = db.GetCollection<ZoneGroup>(TableName);
            objDb.Name = obj.Name;
            objDb.Zones = obj.Zones;
            return collection.Update(objDb);
        }
    }

    public new bool Delete(ZoneGroup obj)
    {
        using (var db = new LiteDatabase(ConnString))
        {
            var collection = db.GetCollection<ZoneGroup>(TableName);
            return collection.Delete(obj.Id);
        }
    }


}