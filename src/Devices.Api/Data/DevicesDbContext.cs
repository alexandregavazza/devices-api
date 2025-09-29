using Devices.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Devices.Api.Data
{
    public class DevicesDbContext : DbContext
    {
        public DevicesDbContext(DbContextOptions<DevicesDbContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Brand).IsRequired().HasMaxLength(100);
                entity.Property(d => d.State).IsRequired();
                entity.Property(d => d.CreationTime).IsRequired();
            });
        }
    }
}