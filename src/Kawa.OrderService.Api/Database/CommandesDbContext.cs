using Kawa.OrderService.Api.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Kawa.OrderService.Api.Database
{
    public class CommandesDbContext : DbContext
    {
        public CommandesDbContext(DbContextOptions<CommandesDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderLine>()
                .HasKey(ol => new { ol.OrderId, ol.ProductId });

            modelBuilder.Entity<OrderLine>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.Lignes)
                .HasForeignKey(ol => ol.OrderId);
        }
    }
}
