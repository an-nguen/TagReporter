using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TagReporter.Models;

namespace TagReporter.Contracts.Repositories;

public interface ITagRepository
{
    public List<Tag> FindAll();
    public Task<List<Tag>> FindAllAsync();
    public Task Create(Tag tag);
    public Task Update(Guid uuid, Tag tag);
    public Task Delete(Guid uuid, Tag tag);

    public void RemoveAll();

    public Task StoreTagsFromCloud(WstAccount wstAccount);
}