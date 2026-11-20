using Microsoft.EntityFrameworkCore;
using VoyageHub.Data.Entities;

namespace VoyageHub.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TravelPolicy> TravelPolicies => Set<TravelPolicy>();
    public DbSet<TravelRequest> TravelRequests => Set<TravelRequest>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<ItinerarySegment> ItinerarySegments => Set<ItinerarySegment>();
    public DbSet<Stay> Stays => Set<Stay>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS table
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Users)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Manager)
                  .WithMany(m => m.Subordinates)
                  .HasForeignKey(e => e.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // DEPARTMENTS table
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("DEPARTMENTS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasOne(d => d.Policy)
                  .WithOne(p => p.Department)
                  .HasForeignKey<TravelPolicy>(p => p.DepartmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // TRAVEL_POLICIES table
        modelBuilder.Entity<TravelPolicy>(entity =>
        {
            entity.ToTable("TRAVEL_POLICIES");
            entity.HasKey(e => e.Id);
        });

        // TRAVEL_REQUESTS table
        modelBuilder.Entity<TravelRequest>(entity =>
        {
            entity.ToTable("TRAVEL_REQUESTS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RequestCode).IsUnique();

            entity.HasOne(r => r.Employee)
                  .WithMany(u => u.TravelRequests)
                  .HasForeignKey(r => r.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // APPROVAL_STEPS table
        modelBuilder.Entity<ApprovalStep>(entity =>
        {
            entity.ToTable("APPROVAL_STEPS");
            entity.HasKey(e => e.Id);

            entity.HasOne(s => s.Request)
                  .WithMany(r => r.ApprovalSteps)
                  .HasForeignKey(s => s.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Approver)
                  .WithMany(u => u.ApprovalSteps)
                  .HasForeignKey(s => s.ApproverId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // ITINERARY_SEGMENTS table
        modelBuilder.Entity<ItinerarySegment>(entity =>
        {
            entity.ToTable("ITINERARY_SEGMENTS");
            entity.HasKey(e => e.Id);

            entity.HasOne(s => s.Request)
                  .WithMany(r => r.ItinerarySegments)
                  .HasForeignKey(s => s.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // STAYS table
        modelBuilder.Entity<Stay>(entity =>
        {
            entity.ToTable("STAYS");
            entity.HasKey(e => e.Id);

            entity.HasOne(s => s.Request)
                  .WithMany(r => r.Stays)
                  .HasForeignKey(s => s.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // EXPENSES table
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("EXPENSES");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Request)
                  .WithMany(r => r.Expenses)
                  .HasForeignKey(e => e.RequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // AUDIT_LOGS table
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AUDIT_LOGS");
            entity.HasKey(e => e.Id);

            entity.HasOne(a => a.Actor)
                  .WithMany()
                  .HasForeignKey(a => a.ActorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // SUPPORT_TICKETS table
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.ToTable("SUPPORT_TICKETS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TicketCode).IsUnique();

            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Request)
                  .WithMany()
                  .HasForeignKey(t => t.RequestId)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
