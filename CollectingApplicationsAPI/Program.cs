using CollectingApplicationsAPI.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CollectingApplicationsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IApplicationProvider, ApplicationsProvider>();
            // builder.Configuration.GetConnectionString("ConnectionString");

            var connectionString = builder.Configuration.GetConnectionString("ConnectionString");

            builder.Services.AddDbContext<ApplicationContext>(options =>
            {
                options
                    .UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString"),
                        assembly =>
                            assembly.MigrationsAssembly("CollectingApplicationsAPI"));
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
