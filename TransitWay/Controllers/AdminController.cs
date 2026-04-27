using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;
using System.Security.Claims;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
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
                a.Status,
                a.Role,
                photoUrl = a.PhotoPath != null
                    ? $"{Request.Scheme}://{Request.Host}/uploads/admins/{a.PhotoPath}"
                    : null
            })
            .ToList();
        return Ok(admins);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var admin = _context.Admins.Find(id);
        if (admin == null)
            return NotFound(new { message = "Admin not found" });

        return Ok(new
        {
            admin.Id,
            admin.Code,
            admin.FullName,
            admin.Email,
            admin.PhoneNumber,
            admin.Status,
            admin.Role,
            photoUrl = admin.PhotoPath != null
                ? $"{Request.Scheme}://{Request.Host}/uploads/admins/{admin.PhotoPath}"
                : null
        });
    }

    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile photo)
    {
        var admin = _context.Admins.Find(id);
        if (admin == null)
            return NotFound(new { message = "Admin not found" });

        if (photo == null || photo.Length == 0)
            return BadRequest(new { message = "No photo provided" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
        if (!allowedTypes.Contains(photo.ContentType.ToLower()))
            return BadRequest(new { message = "Only JPG and PNG images are allowed" });

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "admins");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        if (!string.IsNullOrEmpty(admin.PhotoPath))
        {
            var oldPath = Path.Combine(uploadsFolder, admin.PhotoPath);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileName = $"admin_{id}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        admin.PhotoPath = fileName;
        _context.SaveChanges();

        return Ok(new
        {
            message = "Photo uploaded successfully",
            photoUrl = $"{Request.Scheme}://{Request.Host}/uploads/admins/{fileName}"
        });
    }

    [HttpPost]
    public IActionResult Create(CreateAdminDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "SuperAdmin")
            return Forbid("Only SuperAdmin can create admins");

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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Status = "Active",
            Role = dto.Role ?? "Admin"
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
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "SuperAdmin")
            return Forbid("Only SuperAdmin can update");

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
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "SuperAdmin")
            return Forbid("Only SuperAdmin can change status");

        var admin = _context.Admins.Find(id);
        if (admin == null)
            return NotFound();

        admin.Status = admin.Status == "Active" ? "Inactive" : "Active";
        _context.SaveChanges();

        return Ok(new { status = admin.Status });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != "SuperAdmin")
            return Forbid("Only SuperAdmin can delete");

        var admin = _context.Admins.Find(id);
        if (admin == null)
            return NotFound();

        if (!string.IsNullOrEmpty(admin.PhotoPath))
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "admins");
            var oldPath = Path.Combine(uploadsFolder, admin.PhotoPath);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        _context.Admins.Remove(admin);
        _context.SaveChanges();

        return Ok("Deleted successfully");
    }
}