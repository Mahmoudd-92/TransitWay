using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;

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
 
    [HttpGet]
    public IActionResult GetAll()
    {
        var admins = _context.Admins
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.FullName,
                a.Email,
                a.PhoneNumber,
                a.Status
            })
            .ToList();

        return Ok(admins);
    }


    [HttpPost]
    public IActionResult Create(CreateAdminDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        int count = _context.Admins.Count() + 1;
        string code = $"A{count:D3}";

        var admin = new Admin
        {
            Code = code,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = dto.Password, 
            Status = "Active"
        };

        _context.Admins.Add(admin);
        _context.SaveChanges();

        return Ok(new
        {
            message = "Admin created successfully",
            adminId = admin.Id
        });
    }

    
    [HttpPut("{id}")]
    public IActionResult Update(int id, UpdateAdminDto dto)
    {
        var admin = _context.Admins.Find(id);

        if (admin == null)
            return NotFound("Admin not found");

        admin.FullName = dto.FullName;
        admin.Email = dto.Email;
        admin.PhoneNumber = dto.PhonNumber;

        _context.SaveChanges();

        return Ok("Updated successfully");
    }

  
    [HttpPut("status/{id}")]
    public IActionResult ToggleStatus(int id)
    {
        var admin = _context.Admins.Find(id);

        if (admin == null)
            return NotFound();

        admin.Status = admin.Status == "Active"
            ? "Inactive"
            : "Active";

        _context.SaveChanges();

        return Ok(new
        {
            status = admin.Status
        });
    }


    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var admin = _context.Admins.Find(id);

        if (admin == null)
            return NotFound();

        _context.Admins.Remove(admin);
        _context.SaveChanges();

        return Ok("Deleted successfully");
    }
}