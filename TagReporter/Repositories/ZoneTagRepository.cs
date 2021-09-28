using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using TagReporter.Models;

namespace TagReporter.Repositories
{
    public class ZoneTagRepository: BaseRepository<ZoneTag>
    {

        public ZoneTagRepository() : base("zone_tag", CommonResources.ConnectionString)
        {

        }

        
        public new bool Update(ZoneTag objDb, ZoneTag obj)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<ZoneTag>(TableName);
            objDb.TagUuid = obj.TagUuid;
            objDb.ZoneUuid = obj.ZoneUuid;
            return collection.Update(objDb);
        }

        public new bool Delete(ZoneTag obj)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Zone>(TableName);
            return collection.Delete(obj.Id);
        }
    }
}
