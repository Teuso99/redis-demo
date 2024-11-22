using Microsoft.EntityFrameworkCore;

namespace RedisDemo;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            var users = Seed.SeedUsers();
            var userCount = await context.Set<User>().CountAsync(cancellationToken);

            if (userCount == 0)
            {
                await context.Set<User>().AddRangeAsync(users, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        });
    }
}