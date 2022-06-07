using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;

namespace TagReporter.Repositories;

public class ZoneRepository: IZoneRepository
{
    private readonly AppDbContext _dbContext;
    private List<ZoneTagUuid> ZoneTagUuids { get; set; } = new();
    private List<Tag> Tags { get; set; } = new();

    public ZoneRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        UpdateInMemoryData();
    }

    public void UpdateInMemoryData()
    {
        ZoneTagUuids = _dbContext.ZoneTagUuids.ToList();
        Tags = _dbContext.Tags.ToList();
    }

    public List<Zone> FindAll()
    {
        var zones = _dbContext.Zones.ToList();
        foreach (var z in zones)
        {
            z.Tags = FindTagsByZone(z);
        }

        return zones;
    }

    public async Task<List<Zone>> FindAllAsync()
    {
        var zones = await _dbContext.Zones.ToListAsync();
        foreach (var z in zones) z.Tags = await FindTagsByZoneAsync(z);
        return zones;
    }

    public List<Tag> FindTagsByZone(Zone zone)
    {
        var uuids = ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id).Select(ztu => ztu.TagUuid).ToList();
        return Tags.Where(t => uuids.Contains(t.Uuid)).ToList();
    }

    public async Task<List<Tag>> FindTagsByZoneAsync(Zone zone)
    {
        var uuids = await _dbContext.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id).Select(ztu => ztu.TagUuid).ToListAsync();
        return await _dbContext.Tags.Where(t => uuids.Contains(t.Uuid)).ToListAsync();
    }
    public List<Guid> FindTagUuidsByZone(Zone zone)
    {
        var uuids = ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id).Select(ztu => ztu.TagUuid).ToList();
        return _dbContext.Tags.Where(t => uuids.Contains(t.Uuid)).Select(t => t.Uuid).ToList();
    }

    public async Task Create(Zone zone)
    {
        if (string.IsNullOrEmpty(zone.Name)) throw new Exception("[Create] zone.Name is empty");
        _dbContext.Zones.Add(zone);
        await _dbContext.SaveChangesAsync();
        await _dbContext.AddRangeAsync(zone.TagUuids.Select(uuid => new ZoneTagUuid
        {
            ZoneId = zone.Id,
            TagUuid = uuid
        }));
        await _dbContext.SaveChangesAsync();
        UpdateInMemoryData();
    }

    public async Task Update(int? id, Zone zone)
    {
        var updZone = await _dbContext.Zones.FindAsync(id);
        if (updZone == null) throw new Exception($"[Update] Zone with {id} not found!");
        updZone.Name = zone.Name;
        _dbContext.Update(updZone);
        await _dbContext.SaveChangesAsync();
        _dbContext.ZoneTagUuids.RemoveRange(
            _dbContext.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == updZone.Id));
        await _dbContext.SaveChangesAsync();
        await _dbContext.AddRangeAsync(zone.TagUuids.Select(uuid => new ZoneTagUuid
        {
            ZoneId = updZone.Id,
            TagUuid = uuid
        }));
        await _dbContext.SaveChangesAsync();
        UpdateInMemoryData();
    }

    public async Task Delete(int id)
    {
        var zone = await _dbContext.Zones.FindAsync(id);
        if (zone == null) throw new Exception("[Delete] ");
        _dbContext.ZoneTagUuids.RemoveRange(
            _dbContext.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id));
        _dbContext.Zones.Remove(zone);
        await _dbContext.SaveChangesAsync();
        UpdateInMemoryData();
    }

}