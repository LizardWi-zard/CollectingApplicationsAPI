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
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
        public DbSet<T> DbSet<T>() where T : class
        {
            return Set<T>();
        }
        public new IQueryable<T> Query<T>() where T : class
        {
            return Set<T>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activity>().HasData(
                new Activity[]
                {
                new Activity {Id = 1, Name="Report", Description = "Доклад, 35-45 минут"},
                new Activity {Id = 2, Name="Masterclass", Description = "Мастеркласс, 1-2 часа"},
                new Activity {Id = 3, Name="Discussion", Description = "Дискуссия / круглый стол, 40-50 минут"}
                });
        }
    }
}
