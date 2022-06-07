using Microsoft.EntityFrameworkCore;
using TagReporter.Models;

namespace TagReporter;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<Tag> Tags { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<WstAccount> WstAccounts { get; set; }
    public DbSet<ZoneTagUuid> ZoneTagUuids { get; set; }
}
