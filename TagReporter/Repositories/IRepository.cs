using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TagReporter.Repositories
{
    public interface IRepository<T>
    {
        List<T> FindAll();
        BsonValue Create(T obj);
        bool Update(T objDb, T obj);
        bool Delete(T obj);
    }
}
