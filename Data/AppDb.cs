using AuthDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthDemo.Data;

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b) =>
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
}
