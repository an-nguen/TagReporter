using LiteDB;
using TagReporter.Models;

namespace TagReporter.Repositories
{
    public class AccountRepository: BaseRepository<Account>
    {
        public AccountRepository() : base("account", CommonResources.ConnectionString)
        {

        }

        public new bool Update(Account objDb, Account obj)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Account>(TableName);
            objDb.Email = obj.Email;
            objDb.Password = obj.Password;

            return collection.Update(objDb);
        }

        public new bool Delete(Account tag)
        {
            using var db = new LiteDatabase(ConnString);
            var collection = db.GetCollection<Account>(TableName);
            return collection.Delete(tag.Id);
        }
    }
}
