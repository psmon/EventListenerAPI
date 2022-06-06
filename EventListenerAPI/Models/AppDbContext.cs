using Microsoft.EntityFrameworkCore;

namespace EventListenerAPI.Models
{
    public class AppDbContext : DbContext  
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)  
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);  
        }  
        public DbSet<EventLog> EventLogs { get; set; }  
    }
}
