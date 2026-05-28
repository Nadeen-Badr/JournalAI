using JournalAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalAI.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .HasMaxLength(100);

        modelBuilder.Entity<JournalEntry>()
            .Property(j => j.Title)
            .HasMaxLength(200);
    }
}