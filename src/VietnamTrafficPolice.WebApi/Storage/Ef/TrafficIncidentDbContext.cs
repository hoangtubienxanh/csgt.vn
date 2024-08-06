using Microsoft.EntityFrameworkCore;

namespace VietnamTrafficPolice.WebApi.Storage.Ef;

public partial class TrafficIncidentDbContext : DbContext
{
    public TrafficIncidentDbContext()
    {
    }

    public TrafficIncidentDbContext(DbContextOptions<TrafficIncidentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DistributedCache> DistributedCaches { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_uca1400_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<DistributedCache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("DistributedCache");

            entity.HasIndex(e => e.ExpiresAtTime, "Index_ExpiresAtTime");

            entity.Property(e => e.Id)
                .HasMaxLength(449)
                .UseCollation("ascii_bin")
                .HasCharSet("ascii");
            entity.Property(e => e.AbsoluteExpiration).HasMaxLength(6);
            entity.Property(e => e.ExpiresAtTime).HasMaxLength(6);
            entity.Property(e => e.SlidingExpirationInSeconds).HasColumnType("bigint(20)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}