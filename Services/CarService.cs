using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public bool DoesCarExist(long carId)
    {
        return _db.Cars.Any(c => c.Id == carId);
    }

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    public async Task<bool> RegisterClaimInsuranceAsync(long carId, DateOnly date, string description, long amount)
    {
        var isInsuranceValid = await IsInsuranceValidAsync(carId, date);

        if(isInsuranceValid)
        {
            _db.Claims.Add(new Claims
            {
                CarId = carId,
                ClaimDate = date,
                Description = description,
                Amount = amount
            });

            _db.SaveChanges();
        }

        return isInsuranceValid;
    }

    public List<CarHistoryItem> GetCarHistory(long carId)
    {
        var policies = _db.Policies
            .Where(p => p.CarId == carId)
            .Select(p => new CarHistoryItem
            {
                Type = "Policy",
                Date = p.StartDate,
                PolicyId = p.Id,
                Provider = p.Provider,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToListAsync();

        var claims = _db.Claims
            .Where(c => c.CarId == carId)
            .Select(c => new CarHistoryItem
            {
                Type = "Claim",
                Date = c.ClaimDate,
                ClaimId = c.Id,
                ClaimDate = c.ClaimDate,
                Description = c.Description,
                Amount = c.Amount
            }).ToListAsync();

        var history = policies.Result.Concat(claims.Result).OrderBy(d => d.Date).ThenBy(t => t.Type).ToList();

        return history;
    }
}
