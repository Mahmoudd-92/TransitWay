using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Services;
using TransitWay.Services.AttachmentService;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly GoogleAuthService _googleService;
        private readonly IAttachmentService _attachmentService;
        public AuthController(ApplicationDbContext context, EmailService emailService, GoogleAuthService googleService, IAttachmentService attachmentService)
        {
            _context = context;
            _emailService = emailService;
            _googleService = googleService;
            _attachmentService = attachmentService;
        }

        private string? BuildUserPhotoUrl(string? photoName)
        {
            if (string.IsNullOrWhiteSpace(photoName))
                return null;

            return $"{Request.Scheme}://{Request.Host}/images/users/{photoName}";
        }

        [HttpPost("user/register")]
        public IActionResult UserRegister([FromForm] UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            if (_context.Users.Any(u => u.Phone == dto.Phone))
                return BadRequest("Phone already exists.");

            if (dto.Photo == null || dto.Photo.Length == 0)
                return BadRequest("User photo is required.");

            var photoName = _attachmentService.Upload("users", dto.Photo);
            if (string.IsNullOrWhiteSpace(photoName))
                return BadRequest("Invalid photo. Only jpg, jpeg, png up to 5MB are allowed.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Photo = photoName,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User registered successfully.",
                photo = BuildUserPhotoUrl(user.Photo)
            });
        }



        [HttpPost("driver/register")]
        public IActionResult DriverRegister([FromForm] DriverRegisterDto dto)
            {
            if (_context.Drivers.Any(d => d.Email == dto.Email))
                return BadRequest("Email already exists.");

            if (_context.Drivers.Any(d => d.Phone == dto.Phone))
                return BadRequest("Phone already exists.");
            if (dto.Photo == null || dto.Photo.Length == 0)
                return BadRequest("Driver photo is required.");

            var photoName = _attachmentService.Upload("drivers", dto.Photo);
            if (string.IsNullOrWhiteSpace(photoName))
                return BadRequest("Invalid photo. Only jpg, jpeg, png up to 5MB are allowed.");

            var driver = new Driver
            {
                Name = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                LicenseNumber = dto.LicenseNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Photo = photoName
            };

            _context.Drivers.Add(driver);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Driver registered successfully.",
                photo = $"/images/drivers/{photoName}"
            });
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.IsBanned)
                return StatusCode(403, new
                {
                    message = "Your account has been suspended",
                    reason = user.BanReason,
                    bannedAt = user.BannedAt
                });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid password" });

            return Ok(new
            {
                Type = "User",
                message = "Login successful",
                user.Id,
                user.FullName,
                user.Email,
                Photo = BuildUserPhotoUrl(user.Photo),
               user.Phone
            });
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
                FullName = "Main Admin",
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
