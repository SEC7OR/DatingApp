using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<UserLike> Likes { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups{ get; set; }
    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(u => u.UserId)
            .IsRequired();


        modelBuilder.Entity<AppRole>()
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(u => u.RoleId)
            .IsRequired();

        modelBuilder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetUserId });

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


        modelBuilder.Entity<Message>()
            .HasOne(m => m.Recipient)
            .WithMany(r => r.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(s => s.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);


    }
}
