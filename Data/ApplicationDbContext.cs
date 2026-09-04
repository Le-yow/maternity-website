using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Models;

namespace MaterniTrack.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ClinicProfileSetting> ClinicSettings => Set<ClinicProfileSetting>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure indexes
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.AssignedStaff, a.AppointmentDate, a.AppointmentTime });

        builder.Entity<InventoryItem>()
            .HasIndex(i => i.Category);

        builder.Entity<Patient>()
            .HasIndex(p => p.FullName);
    }
}
