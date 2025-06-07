using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QL_Spa.Models;

namespace QL_Spa.Data
{
    public static class SpaDbContextConfiguration
    {
        public static void ConfigureDecimalProperties(ModelBuilder modelBuilder)
        {
            // Configure Invoice entity decimal properties
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.FinalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.PaidAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Configure Service entity decimal properties
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
