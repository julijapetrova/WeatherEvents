using Microsoft.EntityFrameworkCore;
using WeatherEvents.Models;

namespace WeatherEvents.Data;

public class AppDbContext : DbContext
{
    // Constructor to accept DbContextOptions
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet for your WeatherEvent model
    public DbSet<WeatherEvent> WeatherEvents { get; set; }

    // Optional: Configure model relationships or constraints here
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Id as the primary key (auto-generated)
        modelBuilder.Entity<WeatherEvent>()
            .HasKey(e => e.Id);

        // Example: Configure property constraints
        modelBuilder.Entity<WeatherEvent>()
            .Property(e => e.StationId)
            .IsRequired()
            .HasMaxLength(50);
        
        modelBuilder.Entity<WeatherEvent>(entity =>
        {
            entity.Property(e => e.Temperature).HasPrecision(5, 2);
            entity.Property(e => e.Humidity).HasPrecision(5, 2);
            entity.Property(e => e.Pressure).HasPrecision(7, 2);
            entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
        });
    }
}