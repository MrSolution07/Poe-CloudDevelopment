using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventEaseApp.Data;

public class EventEaseDesignTimeDbContextFactory : IDesignTimeDbContextFactory<EventEaseContext>
{
    public EventEaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EventEaseContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=EventEaseDesign;Trusted_Connection=True;");
        return new EventEaseContext(optionsBuilder.Options);
    }
}
