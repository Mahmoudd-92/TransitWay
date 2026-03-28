using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Services;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly GoogleAuthService _googleService;
        public AuthController(ApplicationDbContext context , EmailService emailService, GoogleAuthService googleService)
        {
            _context = context;
            _emailService = emailService;
            _googleService = googleService;
        }


        [HttpPost("user/register")]
        public IActionResult UserRegister([FromBody] RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully.");
        }



        [HttpPost("driver/register")]
        public IActionResult DriverRegister([FromBody] RegisterDto dto)
        {
            if (_context.Drivers.Any(d => d.Email == dto.Email))
                return BadRequest("Email already exists.");

            var driver = new Driver
            {
                Name = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Drivers.Add(driver);
            _context.SaveChanges();

            return Ok("Driver registered successfully.");
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Ok(new
                {
                    Type = "User",
                    user.Id,
                    user.FullName,
                    user.Email
                });
            }

            var driver = _context.Drivers.FirstOrDefault(d => d.Email == dto.Email);
            if (driver != null && BCrypt.Net.BCrypt.Verify(dto.Password, driver.PasswordHash))
            {
                return Ok(new
                {
                    Type = "Driver",
                    driver.Id,
                    driver.Name,
                    driver.Email
                });
            }

            return Unauthorized("Invalid email or password");
        }

        [HttpPost("get-email")]
        public IActionResult GetEmail(PhoneRequestDto input)
        {
            var user = _context.Users.FirstOrDefault(d => d.Phone == input.PhoneNumber);
            if (user == null) return NotFound("Phone Number Not Found");
            return Ok(new
            {
                email = user.Email,
            });
        }

        [HttpPost("request-reset")]
        public IActionResult RequestPasswordReset([FromBody] ResetRequestDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            var driver = _context.Drivers.FirstOrDefault(d => d.Email == dto.Email);

            if (user == null && driver == null)
                return BadRequest("Email not found");

            var code = new Random().Next(100000, 999999).ToString();

            var reset = new PasswordResetCode
            {
                Email = dto.Email,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };

            _context.PasswordResetCodes.Add(reset);
            _context.SaveChanges();

            _emailService.SendEmail(dto.Email, code);

            return Ok(new { message = "Reset code sent" });
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyCodeDto dto)
        {
            var reset = _context.PasswordResetCodes
                .FirstOrDefault(r => r.Email == dto.Email && r.Code == dto.Code);

            if (reset == null || reset.ExpirationTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired code");

            return Ok(new { message = "Code is valid" });
        }


        [HttpPost("confirm-reset")]
        public IActionResult ConfirmReset([FromBody] ConfirmResetDto dto)
        {
            var reset = _context.PasswordResetCodes
                .FirstOrDefault(r => r.Email == dto.Email && r.Code == dto.Code);

            if (reset == null || reset.ExpirationTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired code");

            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            var driver = _context.Drivers.FirstOrDefault(d => d.Email == dto.Email);

            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }
            else if (driver != null)
            {
                driver.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            }
            else
            {
                return BadRequest("User/Driver not found");
            }

            _context.PasswordResetCodes.Remove(reset);
            _context.SaveChanges();

            return Ok(new { message = "Password updated successfully" });
        }



        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var payload = await _googleService.VerifyToken(dto.IdToken);

            var user = _context.Users.FirstOrDefault(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                _context.SaveChanges();
            }

            return Ok(new
            {
                Type = "User",
                user.Id,
                user.Email,
                user.FullName
            });
        }
        [HttpPost("admin/login")]
        public IActionResult AdminLogin([FromBody] LoginDto dto)
        {
            var admin = _context.Admins.FirstOrDefault(u => u.Email == dto.Email);
            if (admin != null && BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            {
                return Ok(new
                {
                    Type = "Admin",
                    admin.Id,
                    admin.Email
                });
            }
            return Unauthorized("Invalid email or password");
        }
        [HttpGet("create-admin")]
        public IActionResult CreateAdmin()
        {
            var admin = new Admin
            {
                Name = "Main Admin",
                Email = "admin123@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@12345"),
                Role = "Admin"
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();

            return Ok("Admin created");
        }
        [HttpPost("Admin-request-reset")]
        public IActionResult AdminRequestReset([FromBody] ResetRequestDto dto)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == dto.Email);

            if (admin == null)
                return BadRequest("Admin email not found");

            var code = new Random().Next(100000, 999999).ToString();

            var reset = new PasswordResetCode
            {
                Email = dto.Email,
                Code = code,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10)
            };

            _context.PasswordResetCodes.Add(reset);
            _context.SaveChanges();

            _emailService.SendEmail(dto.Email, code);

            return Ok("Reset code sent to admin email");
        }
        [HttpPost("Admin-confirm-reset")]
        public IActionResult AdminConfirmReset([FromBody] ConfirmResetDto dto)
        {
            var reset = _context.PasswordResetCodes
                .FirstOrDefault(r => r.Email == dto.Email && r.Code == dto.Code);

            if (reset == null || reset.ExpirationTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired code");

            var admin = _context.Admins.FirstOrDefault(a => a.Email == dto.Email);

            if (admin == null)
                return BadRequest("Admin not found");

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            _context.PasswordResetCodes.Remove(reset);
            _context.SaveChanges();

            return Ok("Admin password updated successfully");
        }
    }
}
