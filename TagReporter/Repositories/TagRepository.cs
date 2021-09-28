using System;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using TagReporter.Models;

namespace TagReporter.Repositories
{
    public class TagRepository: BaseRepository<Tag>
    {

        public TagRepository() : base("tag", CommonResources.ConnectionString)
        {

        }

        public new bool Update(Tag tagDb, Tag tag)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Tag>(TableName);
            tagDb.Name = tag.Name;
            return collection.Update(tag);
        }

        public new bool Delete(Tag tag)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Tag>(TableName);
            return collection.Delete(tag.Uuid);
        }


        public List<Tag> FindTagsByZone(Zone zone)
        {
            var tags = new List<Tag>();

            using var db = new LiteDatabase(ConnString);
            var zoneTags = db.GetCollection<ZoneTag>("zone_tag").Query().Where(zt => zt.ZoneUuid == zone.Uuid).ToList();
            foreach (var zoneTag in zoneTags)
            {
                var tag = db.GetCollection<Tag>(TableName).FindOne(t => t.Uuid == zoneTag.TagUuid);
                tags.Add(tag);
            }

            return tags;
        }

        public void RemoveAll()
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Tag>(TableName);
            var list = collection.Query().ToList();
            foreach (var tag in list)
            {
                collection.Delete(tag.Uuid);
            }
        }

        public Tag? FindTagsById(Guid uuid)
        {
            using var db = new LiteDatabase(ConnString);
            return db.GetCollection<Tag>(TableName).Find(t => t.Uuid == uuid).FirstOrDefault();
        }
    }
}
