using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!_service.DoesCarExist(carId))
            return NotFound();

        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult<ClaimInsuranceResponse>> ClaimInsurance(long carId, [FromQuery] string date, [FromQuery] string description, [FromQuery] long amount)
    {
        if (!_service.DoesCarExist(carId))
            return NotFound();
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        if (string.IsNullOrWhiteSpace(description))
            return BadRequest("A description must be present.");
        if (amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        try
        {
            var success = await _service.ClaimInsuranceAsync(carId, parsed);
            return Ok(new ClaimInsuranceResponse(carId, parsed.ToString("yyyy-MM-dd"), description, amount, success));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<IEnumerable<CarHistoryItem>>> GetCarHistory(long carId)
    {
        if (!_service.DoesCarExist(carId))
            return NotFound();

        var history = _service.GetCarHistory(carId);

        return Ok(history);
    }
}
