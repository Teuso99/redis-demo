using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Models;
using RedisDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "RedisDemoAPI",
        Description = "Minimal API for testing Redis cache",
        Version = "v1"
    });
});

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
var sqliteConnectionString = builder.Configuration.GetConnectionString("SQLite");

builder.Services.AddStackExchangeRedisCache(options =>
{
   options.Configuration = redisConnectionString;
});

builder.Services.AddSqlite<UserDbContext>(sqliteConnectionString);

var app = builder.Build();

await using var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserDbContext>();
await dbContext.Database.EnsureCreatedAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

const string cacheKey = "users";

app.MapGet("/users", async (UserDbContext context, IDistributedCache cache) =>
    {
        var users = await cache.GetOrSetAsync(cacheKey,
            async () =>
            {
                Console.WriteLine("Fetching data from database");
                return await context.Users.ToListAsync();
            });

        return users;
    })
    .WithName("GetUsers")
    .WithOpenApi();

app.MapPost("/users", async (UserDbContext context, IDistributedCache cache) =>
    {
        var user = Seed.SeedUser();
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        cache.Remove(cacheKey);
        
        return user;
    })
    .WithName("CreateUser")
    .WithOpenApi();

app.Run();