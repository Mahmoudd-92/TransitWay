using GraduationProject.Data;
using GraduationProject.Dtos;
using GraduationProject.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DriverController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 GET ALL DRIVERS
        [HttpGet]
        public IActionResult GetAllDrivers()
        {
            var drivers = _context.Drivers
                .Select(d => new DriverResponseDto
                {
                    Id = d.Id,
                    FullName = d.Name,
                    PhoneNumber = d.Phone,
                    Email = d.Email,
                    LicenseNumber = d.LicenseNumber,
                    BusId = d.BusId
                })
                .ToList();

            return Ok(drivers);
        }

        // 🔹 GET DRIVER BY ID
        [HttpGet("{id}")]
        public IActionResult GetDriverById(int id)
        {
            var driver = _context.Drivers
                .Where(d => d.Id == id)
                .Select(d => new DriverResponseDto
                {
                    Id = d.Id,
                    FullName = d.Name,
                    PhoneNumber = d.Phone,
                    Email = d.Email,
                    LicenseNumber = d.LicenseNumber,
                    BusId = d.BusId
                })
                .FirstOrDefault();

            if (driver == null)
                return NotFound("Driver not found");

            return Ok(driver);
        }

        // 🔹 CREATE DRIVER
        [HttpPost]
        public IActionResult CreateDriver(CreateDriverDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // تحقق إن الإيميل أو الموبايل مش متكرر
            if (_context.Drivers.Any(d =>
                d.Email == input.Email ||
                d.Phone == input.PhoneNumber))
            {
                return BadRequest("Email or Phone already exists");
            }

            var driver = new Driver
            {
                Name = input.FullName,
                Phone = input.PhoneNumber,
                Email = input.Email,
                LicenseNumber = input.LicenseNumber,
                BusId = input.BusId,
                PasswordHash = HashPassword(input.Password) // 🔥 مهم
            };

            _context.Drivers.Add(driver);
            _context.SaveChanges();

            return Ok(new { message = "Driver created successfully" });
        }

        // 🔹 UPDATE DRIVER
        [HttpPut("{id}")]
        public IActionResult UpdateDriver(int id, UpdateDriverDto input)
        {
            var driver = _context.Drivers.FirstOrDefault(d => d.Id == id);

            if (driver == null)
                return NotFound("Driver not found");

            driver.Name = input.FullName;
            driver.Phone = input.PhoneNumber;
            driver.Email = input.Email;

            _context.SaveChanges();

            return Ok(new { message = "Driver updated successfully" });
        }

        // 🔹 DELETE DRIVER
        [HttpDelete("{id}")]
        public IActionResult DeleteDriver(int id)
        {
            var driver = _context.Drivers.FirstOrDefault(d => d.Id == id);

            if (driver == null)
                return NotFound("Driver not found");

            _context.Drivers.Remove(driver);
            _context.SaveChanges();

            return Ok(new { message = "Driver deleted successfully" });
        }

        // 🔹 PASSWORD HASH METHOD
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}