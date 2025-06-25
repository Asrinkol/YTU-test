
using Microsoft.EntityFrameworkCore;
using YTU_test.Models;

namespace YTU_test.Data{

        public class AppDbContext : DbContext{

            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
            public DbSet<WeatherForecast> WeatherForecasts { get; set; }
            public DbSet<User> Users { get; set; }
        }
    }
