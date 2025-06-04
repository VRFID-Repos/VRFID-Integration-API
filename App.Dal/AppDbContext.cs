using App.Entity.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ProcessedBooking> ProcessedBookings { get; set; }
    public DbSet<ProcessedAccessCode> ProcessedAccessCodes { get; set; } // New DbSet for ProcessedAccessCode

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProcessedBookings configuration
        modelBuilder.Entity<ProcessedBooking>(entity =>
        {
            // Set table name
            entity.ToTable("ProcessedBookings");

            // Define primary key
            entity.HasKey(e => e.BookingId);

            // Define required properties
            entity.Property(e => e.BookingId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.ProcessedAt).IsRequired();

            // Optionally, define foreign key constraints if there's a Customer entity
            // entity.HasOne<Customer>().WithMany().HasForeignKey(e => e.CustomerId);
        });

        // ProcessedAccessCodes configuration
        modelBuilder.Entity<ProcessedAccessCode>(entity =>
        {
            // Set table name
            entity.ToTable("ProcessedAccessCodes");

            // Define primary key
            entity.HasKey(e => e.AccessCodeId);

            // Define required properties
            entity.Property(e => e.AccessCodeId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.ProcessedAt).IsRequired();

            // Define optional fields
            entity.Property(e => e.AccessCodeCarRego).HasMaxLength(50);
            entity.Property(e => e.SecurityAreaId).HasMaxLength(50);
            entity.Property(e => e.SecurityAreaName).HasMaxLength(255);
            entity.Property(e => e.BookingId).HasMaxLength(255);
            entity.Property(e => e.BookingName).HasMaxLength(255);
            entity.Property(e => e.GuestId).HasMaxLength(255);
            entity.Property(e => e.GuestName).HasMaxLength(255);

            // Define datetime fields
            entity.Property(e => e.AccessCodePeriodFrom).IsRequired();
            entity.Property(e => e.AccessCodePeriodTo).IsRequired();
        });
    }
}
