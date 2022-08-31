using Microsoft.EntityFrameworkCore;

User? user;
using (ApplicationContext db = new ApplicationContext())
{
    Console.WriteLine("Data before update: ");

    var people = db.users.ToList();
    foreach (var person in people)
        Console.WriteLine($"Name: {person.Name}; Age: {person.Age}");
    Console.WriteLine();

    user = db.users.FirstOrDefault();
}

using (ApplicationContext db = new ApplicationContext())
{
    Console.WriteLine("\nData after update: ");

    if (user is not null)
    {
        user.Name = "Alice";
        user.Age = 30;
        db.users.Update(user);
        db.SaveChanges();

       
    }

    var people = db.users.ToList();
    foreach (var person in people)
        Console.WriteLine($"Name: {person.Name}; Age: {person.Age}");
    Console.WriteLine();
}


class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }
}

class ApplicationContext : DbContext
{
    public DbSet<User> users { get; set; } = null!;

    public ApplicationContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data source=DESKTOP-IQB99HQ;Initial catalog=EFPracDB;Integrated security=True");
    }
}