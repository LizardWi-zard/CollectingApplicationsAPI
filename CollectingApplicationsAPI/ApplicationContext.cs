using CollectingApplicationsAPI.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CollectingApplicationsAPI
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }

        public DbSet<Activity> Activities { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // connect to postgres with connection string from app settings
          //  options.UseNpgsql(Configuration.GetConnectionString("WebApiDatabase"));


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activity>().HasData(
                new Activity[]
                {
                new Activity { Name="Report", Description = "Доклад, 35-45 минут"},
                new Activity { Name="Masterclass", Description = "Мастеркласс, 1-2 часа"},
                new Activity { Name="Discussion", Description = "Дискуссия / круглый стол, 40-50 минут"}
                });
        }
    }
}
