using Microsoft.EntityFrameworkCore;
using pastemyst.Models;

namespace pastemyst.DbContexts;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Pasty> Pasties { get; set; }
    public DbSet<Paste> Pastes { get; set; }

    public DataContext(DbContextOptions<DataContext> contextOptions) : base(contextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Username)
            .IsUnique();

        modelBuilder.Entity<Paste>()
            .HasMany(p => p.Pasties)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Paste>()
            .HasOne(p => p.Owner)
            .WithMany();

        modelBuilder.Entity<Paste>()
            .HasMany(p => p.Stars)
            .WithMany()
            .UsingEntity(j => j.ToTable("stars"));
    }
}