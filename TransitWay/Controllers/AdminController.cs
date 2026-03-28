using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("buses")]
    public IActionResult GetAllBuses()
    {
        var buses = _context.Buses
            .Select(b => new
            {
                LatestLocation = b.BusLocations
                    .OrderByDescending(l => l.LastUpdatedAt)
                    .FirstOrDefault()
            })
            .ToList();

        return Ok(buses);
    }
}