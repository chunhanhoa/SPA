using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LoanSpa.Models;
using QL_Spa.Models;

namespace LoanSpa.Data
{
    public class SpaDbContext : IdentityDbContext
    {
        public SpaDbContext(DbContextOptions<SpaDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Chair> Chairs { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentChair> AppointmentChairs { get; set; }
        public DbSet<AppointmentService> AppointmentServices { get; set; }
        public DbSet<InvoiceService> InvoicesServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite keys
            modelBuilder.Entity<AppointmentChair>()
                .HasKey(ac => new { ac.AppointmentId, ac.ChairId });

            modelBuilder.Entity<AppointmentService>()
                .HasKey(aps => new { aps.AppointmentId, aps.ServiceId });

            modelBuilder.Entity<InvoiceService>()
                .HasKey(ins => new { ins.InvoiceId, ins.ServiceId });

            // Computed column for FinalAmount
            modelBuilder.Entity<Invoice>()
                .Property(i => i.FinalAmount)
                .HasComputedColumnSql("[TotalAmount] - ([TotalAmount] * [Discount] / 100)");
        }
    }
}