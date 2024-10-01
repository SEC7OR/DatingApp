using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserLike>()
            .HasKey(k => new {k.SourceUserId, k.TargetUserId});

        modelBuilder.Entity<UserLike>()
            .HasOne(ul => ul.SourceUser)
            .WithMany(ul => ul.LikedUsers)
            .HasForeignKey(ul => ul.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserLike>()
            .HasOne(ul => ul.TargetUser)
            .WithMany(ul => ul.LikedByUsers)
            .HasForeignKey(ul => ul.TargetUserId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
