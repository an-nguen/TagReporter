using Microsoft.EntityFrameworkCore;
using TagReporter.Models;

namespace TagReporter;

public class AppContext : DbContext
{
    public AppContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<Tag> Tags { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<WstAccount> WstAccounts { get; set; }
    public DbSet<ZoneTagUuid> ZoneTagUuids { get; set; }
}
