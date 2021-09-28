using LiteDB;
using TagReporter.Models;

namespace TagReporter.Repositories
{
    public class ZoneRepository: BaseRepository<Zone>
    {

        public ZoneRepository() : base("zone", CommonResources.ConnectionString)
        {

        }

        
        public new bool Update(Zone zoneDb, Zone zone)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Zone>(TableName);
            zoneDb.Name = zone.Name;
            return collection.Update(zoneDb);
        }

        public new bool Delete(Zone zone)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Zone>(TableName);
            return collection.Delete(zone.Uuid);
        }
    }
}
