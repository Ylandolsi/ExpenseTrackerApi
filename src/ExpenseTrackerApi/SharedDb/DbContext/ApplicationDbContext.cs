using Models.Entities;

namespace SharedDb.DbContext;

using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthUser>(
            entity =>
            {
                entity.HasKey(e => e.userId);

            });
        modelBuilder.Entity<UserProfile>(
            entity =>
            {
                entity.HasKey(e => e.userId);
                entity.HasOne(e => e.AuthUser)
                    .WithOne(au => au.UserProfile)
                    .HasForeignKey<AuthUser>(e => e.userId);
                // in one to one relationship,
                // the entity that has the foreign key is the one that has the reference
                // thats why we precise <AuthUser> in the method
            });

        modelBuilder.Entity<Expense>(
            entity =>
            {
                entity.HasKey(e => e.expenseId);
                entity.HasOne(e => e.UserProfile)
                    .WithMany(u => u.UserExpenses)
                    .HasForeignKey(e => e.userProfilesId);
                
                // in one to many relationship,
                // the one side is the entity that has the foreign key
                // thats why we didnt precise <UserProfile> in the method
            });
    }

    public DbSet<Expense> Expenses { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AuthUser> AuthUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
