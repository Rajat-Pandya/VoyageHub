using Microsoft.EntityFrameworkCore;
using TMS.Data.Entity;

namespace TMS.Data
{
    // DbContext = EF Core's "session" with the database.
    // It knows the connection, tracks changes to entities, and turns LINQ into SQL.
    public class AppDbContext : DbContext
    {
        // The options (which provider + connection string) are passed in from Program.cs.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet<T> = the table. "TravelRequests" becomes the table name.
        public DbSet<TravelRequest> TravelRequests { get; set; }
    }
}
