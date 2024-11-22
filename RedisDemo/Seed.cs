using Bogus;

namespace RedisDemo;

public static class Seed
{
    public static List<User> SeedUsers()
    {
        var userFaker = new Faker<User>(locale: "pt_BR")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FirstName)
            .RuleFor(u => u.Email, f => f.Person.Email);
        
        return userFaker.Generate(5);
    }
    
    public static User SeedUser()
    {
        var userFaker = new Faker<User>(locale: "pt_BR")
            .RuleFor(u => u.Id, f => f.Random.Guid())
            .RuleFor(u => u.Name, f => f.Person.FirstName)
            .RuleFor(u => u.Email, f => f.Person.Email);
        
        return userFaker.Generate();
    }
}