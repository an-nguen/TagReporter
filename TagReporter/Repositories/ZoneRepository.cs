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
    private readonly AppContext _context;
    private List<ZoneTagUuid> ZoneTagUuids { get; set; } = new();
    private List<Tag> Tags { get; set; } = new();

    public ZoneRepository(AppContext context)
    {
        _context = context;
        UpdateInMemoryData();
    }

    public void UpdateInMemoryData()
    {
        ZoneTagUuids = _context.ZoneTagUuids.ToList();
        Tags = _context.Tags.ToList();
    }

    public List<Zone> FindAll()
    {
        var zones = _context.Zones.ToList();
        foreach (var z in zones)
        {
            z.Tags = FindTagsByZone(z);
        }

        return zones;
    }

    public async Task<List<Zone>> FindAllAsync()
    {
        var zones = await _context.Zones.ToListAsync();
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
        var uuids = await _context.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id).Select(ztu => ztu.TagUuid).ToListAsync();
        return await _context.Tags.Where(t => uuids.Contains(t.Uuid)).ToListAsync();
    }
    public List<Guid> FindTagUuidsByZone(Zone zone)
    {
        var uuids = ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id).Select(ztu => ztu.TagUuid).ToList();
        return _context.Tags.Where(t => uuids.Contains(t.Uuid)).Select(t => t.Uuid).ToList();
    }

    public async Task Create(Zone zone)
    {
        if (string.IsNullOrEmpty(zone.Name)) throw new Exception("[Create] zone.Name is empty");
        _context.Add(zone);
        await _context.SaveChangesAsync();
        await _context.AddRangeAsync(zone.TagUuids.Select(uuid => new ZoneTagUuid
        {
            ZoneId = zone.Id,
            TagUuid = uuid
        }));
        await _context.SaveChangesAsync();
        UpdateInMemoryData();
    }

    public async Task Update(int? id, Zone zone)
    {
        if (id == null || id != zone.Id)
            throw new Exception("[Update] Zone Id not equals Id");
        var updZone = await _context.Zones.FindAsync(id);
        if (updZone == null) throw new Exception($"[Update] Zone with {id} not found!");
        updZone.Name = zone.Name;
        _context.Update(updZone);
        await _context.SaveChangesAsync();
        _context.ZoneTagUuids.RemoveRange(
            _context.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == updZone.Id));
        await _context.SaveChangesAsync();
        await _context.AddRangeAsync(zone.TagUuids.Select(uuid => new ZoneTagUuid
        {
            ZoneId = updZone.Id,
            TagUuid = uuid
        }));
        await _context.SaveChangesAsync();
        UpdateInMemoryData();
    }

    public async Task Delete(int id)
    {
        var zone = await _context.Zones.FindAsync(id);
        if (zone == null) throw new Exception("[Delete] ");
        _context.ZoneTagUuids.RemoveRange(
            _context.ZoneTagUuids.Where(zoneTagUuid => zoneTagUuid.ZoneId == zone.Id));
        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();
        UpdateInMemoryData();
    }

}