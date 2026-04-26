using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;

namespace TransitWay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("rate")]
        public IActionResult RateDriver([FromBody] RateDriverDto dto)
        {
            if (dto.Rate < 1 || dto.Rate > 5)
                return BadRequest("Rate must be between 1 and 5");

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == dto.TicketId && t.UserId == dto.UserId);

            if (ticket == null)
                return BadRequest("Invalid ticket");

            if (ticket.TripEndTime == null)
                return BadRequest("You can only rate after completing trip");
            var driver = _context.Drivers
                .FirstOrDefault(d => d.BusId == ticket.BusId);

            if (driver == null)
                return NotFound("Driver not found");

            var existing = _context.driverRatings
                .FirstOrDefault(r => r.TicketId == dto.TicketId);

            if (existing != null)
            {
                existing.Rate = dto.Rate;
                existing.Comment = dto.Comment;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var rating = new DriverRating
                {
                    UserId = dto.UserId,
                    DriverId = driver.Id,
                    TicketId = dto.TicketId,
                    Rate = dto.Rate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.driverRatings.Add(rating);
            }

            _context.SaveChanges();

            var ratings = _context.driverRatings
                .Where(r => r.DriverId == driver.Id)
                .ToList();

            double avgRating = ratings.Any()
                ? ratings.Average(r => r.Rate)
                : 0;

            return Ok(new
            {
                message = "Rating submitted successfully",
                averageRating = Math.Round(avgRating, 1),
                totalRatings = ratings.Count
            });
        }

        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Where(u => u.Email != "manual.passenger@transitway.local")
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    phone = u.Phone,
                    balance = _context.Wallets
                                   .Where(w => w.UserId == u.Id)
                                   .Select(w => w.Balance)
                                   .FirstOrDefault()
                })
                .ToList();

            return Ok(users);
        }



    }
}
