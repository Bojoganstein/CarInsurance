using CarInsurance.Api.Models;
using CarInsurance.Api.ScheduledTasks.PolicyExpiry;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();
    public DbSet<Claims> Claims => Set<Claims>();
    public DbSet<ProcessedExpiration> ProcessedExpirations => Set<ProcessedExpiration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.Vin)
            .IsUnique(false); // TODO: set true and handle conflicts

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.StartDate)
            .IsRequired();

        //Task 1: Added EndDate non-nullable
        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.EndDate)
            .IsRequired();

        //Task 2: Car History
        modelBuilder.Entity<Claims>()
            .Property(p => p.ClaimDate)
            .IsRequired();

        // Task 4 : Scheduled Task

        modelBuilder.Entity<ProcessedExpiration>()
            .HasKey(p => p.PolicyId);

        // EndDate intentionally left nullable for a later task
    }
}

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Owners.Any()) return;

        var ana = new Owner { Name = "Ana Pop", Email = "ana.pop@example.com" };
        var bogdan = new Owner { Name = "Bogdan Ionescu", Email = "bogdan.ionescu@example.com" };
        db.Owners.AddRange(ana, bogdan);
        db.SaveChanges();

        var car1 = new Car { Vin = "VIN12345", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018, OwnerId = ana.Id };
        var car2 = new Car { Vin = "VIN67890", Make = "VW", Model = "Golf", YearOfManufacture = 2021, OwnerId = bogdan.Id };
        db.Cars.AddRange(car1, car2);
        db.SaveChanges();

        //Added EndDate for InsurancePolicy where Provider is Groupama

        //Testing Task 1,2,3
        db.Policies.AddRange(
            new InsurancePolicy { CarId = car1.Id, Provider = "Allianz", StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31) },
            new InsurancePolicy { CarId = car1.Id, Provider = "Groupama", StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2026, 1, 1) }, // open-ended on purpose (Task 1: Added mandatory EndDate)
            new InsurancePolicy { CarId = car2.Id, Provider = "Allianz", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2028, 12, 31) }
        );
        db.SaveChanges();

        //Testing Task 4
        //db.Policies.AddRange(
        //    new InsurancePolicy { CarId = car1.Id, Provider = "Allianz", StartDate = new DateOnly(2024,1,1), EndDate = new DateOnly(2024,12,31) },
        //    new InsurancePolicy { CarId = car1.Id, Provider = "Groupama", StartDate = new DateOnly(2025,1,1), EndDate = new DateOnly(2026, 1, 1) }, // open-ended on purpose (Task 1: Added mandatory EndDate)
        //    new InsurancePolicy { CarId = car2.Id, Provider = "Allianz", StartDate = new DateOnly(2025,1,1), EndDate = new DateOnly(2025,6,1) },
        //    new InsurancePolicy { CarId = car2.Id, Provider = "Groupama", StartDate = new DateOnly(2025, 6, 1), EndDate = new DateOnly(2025, 9, 5) }
        //);
        //db.SaveChanges();

        //Task 2 : Added Claims data
        db.Claims.AddRange(
            new Claims { CarId = car1.Id, ClaimDate = new DateOnly(2024, 6, 1), Description = "Minor accident", Amount = 6969 },
            new Claims { CarId = car1.Id, ClaimDate = new DateOnly(2025, 6, 1), Description = "Windshield replacement", Amount = 1234 },
            new Claims { CarId = car2.Id, ClaimDate = new DateOnly(2026, 6, 1), Description = "Theft", Amount = 666 }
        );
        db.SaveChanges();

    }
}
