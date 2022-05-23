using System;
using System.Collections.Generic;
using LiteDB;
using TagReporter.Contracts.Repositories;

namespace TagReporter.Repositories;

public class LiteDbBaseRepository<T> : IRepository<T>
{
    public readonly string TableName;
    public readonly string ConnString;

    public LiteDbBaseRepository(string tableName, string connString)
    {
        TableName = tableName;
        ConnString = connString;
    }

    public List<T> FindAll()
    {
        using var db = new LiteDatabase(ConnString);
        return db.GetCollection<T>(TableName).Query().ToList();
    }

    public ILiteCollection<T> Collection()
    {
        using var db = new LiteDatabase(ConnString);
        return db.GetCollection<T>(TableName);
    }

    public ILiteQueryable<T> Query()
    {
        using var db = new LiteDatabase(ConnString);
        return db.GetCollection<T>(TableName).Query();
    }

    public BsonValue Create(T obj)
    {
        using var db = new LiteDatabase(ConnString);
        var col = db.GetCollection<T>(TableName);
        return col.Insert(obj);
    }

    public bool Update(T objDb, T obj)
    {
        throw new NotImplementedException();
    }

    public bool Delete(T obj)
    {
        throw new NotImplementedException();
    }

}