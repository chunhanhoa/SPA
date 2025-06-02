using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QL_Spa.Models;

namespace QL_Spa.Data
{
    public class SpaDbContext : IdentityDbContext
    {
        public SpaDbContext(DbContextOptions<SpaDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Chair> Chairs { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<AppointmentChair> AppointmentChairs { get; set; }
        public virtual DbSet<AppointmentService> AppointmentServices { get; set; }
        public virtual DbSet<InvoiceService> InvoicesServices { get; set; }

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

            // Configure entity relationships and constraints
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomId);
                entity.Property(e => e.RoomName).HasMaxLength(50);
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            });
        }
    }
}