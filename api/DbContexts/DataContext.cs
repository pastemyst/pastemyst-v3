using Microsoft.EntityFrameworkCore;
using Npgsql;
using pastemyst.Models;

namespace pastemyst.DbContexts;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Pasty> Pasties { get; set; }
    public DbSet<Paste> Pastes { get; set; }
    public DbSet<ActionLog> ActionLogs { get; set; }

    public DataContext(DbContextOptions<DataContext> contextOptions) : base(contextOptions)
    {
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ExpiresIn>();
        NpgsqlConnection.GlobalTypeMapper.MapEnum<ActionLogType>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");

        modelBuilder.HasPostgresEnum<ExpiresIn>();
        modelBuilder.HasPostgresEnum<ActionLogType>();

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Settings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSettings>(s => s.UserId);

        modelBuilder.Entity<UserSettings>()
            .HasKey(u => u.UserId);

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
