using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TagReporter.Models;

namespace TagReporter.Contracts.Repositories;

public interface IZoneRepository
{
    public List<Zone> FindAll();
    public Task<List<Zone>> FindAllAsync();
    public List<Tag> FindTagsByZone(Zone zone);
    public Task<List<Tag>> FindTagsByZoneAsync(Zone zone);
    public List<Guid> FindTagUuidsByZone(Zone zone);
    public Task Create(Zone zone);
    public Task Update(int? id, Zone zone);

    public Task Delete(int id);
}