using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos.Settings;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
      private ApplicationDbContext _context;
        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPut("Update Profile")]
        public IActionResult UpdateProfile(UpdateProfileDto dto)
        {
            var user = _context.Users.Find(dto.UserId);
            if (user == null)
                return NotFound("User Not Found");
            user.FullName = dto.DisplayName;
            user.Email = dto.Email;
            user.Phone = dto.PhoneNumber;
            _context.SaveChanges();
            return Ok(new
            {
                message = "Profile Updated Successfully",
                user.FullName, 
                user.Email, 
                user.Phone
            });
        }

        [HttpGet("profile/{userId}")]
        public IActionResult GetProfile(int userId)
        {
            var user = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    displayName = u.FullName,
                    phoneNumber = u.Phone,
                    email = u.Email,
                })
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Logged out successfully"
            });
        }
    }
}
