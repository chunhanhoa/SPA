using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace YourNamespace.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Thêm quan hệ giữa Invoice và InvoiceDetail
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.InvoiceDetails)
                .WithOne(d => d.Invoice)
                .HasForeignKey(d => d.InvoiceId);
        }
    }

    public class Invoice
    {
        public int Id { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }

    public class InvoiceDetail
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
    }
}