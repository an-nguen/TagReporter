using System.Collections.Generic;
using LiteDB;

namespace TagReporter.Contracts.Repositories;

public interface IRepository<T>
{
    List<T> FindAll();
    BsonValue Create(T obj);
    bool Update(T objDb, T obj);
    bool Delete(T obj);
}