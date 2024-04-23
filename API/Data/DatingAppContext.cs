using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DatingAppContext : DbContext
{
    public DatingAppContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<AppUser> Users { get; set; }
}
