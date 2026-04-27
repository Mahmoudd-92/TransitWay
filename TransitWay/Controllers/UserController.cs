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

        private string? BuildUserPhotoUrl(string? photoName)
        {
            if (string.IsNullOrWhiteSpace(photoName))
                return null;

            return $"{Request.Scheme}://{Request.Host}/images/users/{photoName}";
        }

        private void CreateNotification(int userId, string title, string body)
        {
            _context.UserNotifications.Add(new UserNotification
            {
                UserId = userId,
                Title = title,
                Body = body,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        [HttpGet("{userId}/notifications")]
        public IActionResult GetNotifications(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var notifications = _context.UserNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    body = n.Body,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                userId = userId,
                unreadCount = notifications.Count(n => !n.isRead),
                notifications = notifications
            });
        }

        [HttpPost("{userId}/notifications/{notificationId}/read")]
        public IActionResult MarkAsRead(int userId, int notificationId)
        {
            var notification = _context.UserNotifications
                .FirstOrDefault(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return NotFound(new { message = "Notification not found" });

            notification.IsRead = true;
            _context.SaveChanges();

            return Ok(new { message = "Marked as read" });
        }

        [HttpPost("{userId}/notifications/read-all")]
        public IActionResult MarkAllAsRead(int userId)
        {
            var unread = _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var n in unread)
                n.IsRead = true;

            _context.SaveChanges();

            return Ok(new { message = "All notifications marked as read" });
        }

        [HttpPost("rate")]
        public IActionResult RateDriver([FromBody] RateDriverDto dto)
        {
            if (dto.Rate < 1 || dto.Rate > 5)
                return BadRequest(new { message = "Rate must be between 1 and 5" });

            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == dto.TicketId && t.UserId == dto.UserId);

            if (ticket == null)
                return BadRequest(new { message = "Invalid ticket" });

            if (ticket.TripEndTime == null)
                return BadRequest(new { message = "You can only rate after completing trip" });

            var driver = _context.Drivers
                .FirstOrDefault(d => d.BusId == ticket.BusId);

            if (driver == null)
                return NotFound(new { message = "Driver not found" });

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
                _context.driverRatings.Add(new DriverRating
                {
                    UserId = dto.UserId,
                    DriverId = driver.Id,
                    TicketId = dto.TicketId,
                    Rate = dto.Rate,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            var ratings = _context.driverRatings
                .Where(r => r.DriverId == driver.Id)
                .ToList();

            return Ok(new
            {
                message = "Rating submitted successfully",
                averageRating = Math.Round(ratings.Average(r => r.Rate), 1),
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
                    photo = BuildUserPhotoUrl(u.Photo),
                    isBanned = u.IsBanned,
                    banReason = u.BanReason,
                    bannedAt = u.BannedAt,
                    warningCount = _context.UserWarnings
                        .Count(w => w.UserId == u.Id && w.Type == ActionType.Warning),
                    balance = _context.Wallets
                        .Where(w => w.UserId == u.Id)
                        .Select(w => w.Balance)
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(users);
        }

        [HttpPost("{userId}/warn")]
        public IActionResult WarnUser(int userId, [FromBody] WarnUserDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.IsBanned)
                return BadRequest(new { message = "User is already banned" });

            var currentWarningCount = _context.UserWarnings
                .Count(w => w.UserId == userId && w.Type == ActionType.Warning);

            _context.UserWarnings.Add(new UserWarning
            {
                UserId = userId,
                Reason = dto.Reason,
                Type = ActionType.Warning,
                CreatedAt = DateTime.UtcNow
            });

            var newWarningCount = currentWarningCount + 1;
            var autoBanned = false;

            if (newWarningCount >= 3)
            {
                user.IsBanned = true;
                user.BanReason = "Automatic ban after 3 warnings";
                user.BannedAt = DateTime.UtcNow;
                autoBanned = true;

                _context.UserWarnings.Add(new UserWarning
                {
                    UserId = userId,
                    Reason = "Automatic ban after 3 warnings",
                    Type = ActionType.Ban,
                    CreatedAt = DateTime.UtcNow
                });

                CreateNotification(
                    userId,
                    title: "Account Suspended",
                    body: "Your account has been suspended after 3 warnings."
                );
            }
            else
            {
                CreateNotification(
                    userId,
                    title: $"Warning {newWarningCount}/3",
                    body: $"You received a warning: {dto.Reason}"
                );
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = autoBanned
                    ? "User warned and automatically banned after 3 warnings"
                    : "Warning sent successfully",
                userId = userId,
                reason = dto.Reason,
                warningCount = newWarningCount,
                autoBanned = autoBanned
            });
        }

        [HttpPost("{userId}/ban")]
        public IActionResult BanUser(int userId, [FromBody] BanUserDto dto)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (user.IsBanned)
                return BadRequest(new { message = "User is already banned" });

            user.IsBanned = true;
            user.BanReason = dto.Reason;
            user.BannedAt = DateTime.UtcNow;

            _context.UserWarnings.Add(new UserWarning
            {
                UserId = userId,
                Reason = dto.Reason,
                Type = ActionType.Ban,
                CreatedAt = DateTime.UtcNow
            });

            CreateNotification(
                userId,
                title: "Account Suspended",
                body: $"Your account has been suspended. Reason: {dto.Reason}"
            );

            _context.SaveChanges();

            return Ok(new
            {
                message = "User banned successfully",
                userId = userId,
                reason = dto.Reason,
                bannedAt = user.BannedAt
            });
        }

        [HttpPost("{userId}/unban")]
        public IActionResult UnbanUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            if (!user.IsBanned)
                return BadRequest(new { message = "User is not banned" });

            user.IsBanned = false;
            user.BanReason = null;
            user.BannedAt = null;

            var oldWarnings = _context.UserWarnings
                .Where(w => w.UserId == userId && w.Type == ActionType.Warning)
                .ToList();
            _context.UserWarnings.RemoveRange(oldWarnings);

            CreateNotification(
                userId,
                title: "Account Restored",
                body: "Your account has been restored. You can now use the app again."
            );

            _context.SaveChanges();

            return Ok(new { message = "User unbanned successfully", userId = userId });
        }

        [HttpGet("{userId}/actions")]
        public IActionResult GetUserActions(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found");

            var tickets = _context.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new
                {
                    type = "ticket",
                    date = t.CreatedAt,
                    amount = t.Price,
                    status = t.Status.ToString()
                })
                .ToList();

            var payments = _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new
                {
                    type = "payment",
                    date = p.CreatedAt,
                    amount = p.Amount,
                    status = p.Status
                })
                .ToList();

            var complaints = _context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new
                {
                    type = "complaint",
                    date = c.CreatedAt,
                    category = c.Category,
                    status = c.Status
                })
                .ToList();

            var recentActions = tickets
                .Select(t => new
                {
                    t.type,
                    t.date,
                    amount = (decimal?)t.amount,
                    category = (string?)null,
                    t.status
                })
                .Concat(payments.Select(p => new
                {
                    p.type,
                    p.date,
                    amount = (decimal?)p.amount,
                    category = (string?)null,
                    p.status
                }))
                .Concat(complaints.Select(c => new
                {
                    c.type,
                    c.date,
                    amount = (decimal?)null,
                    category = c.category,
                    c.status
                }))
                .OrderByDescending(a => a.date)
                .Take(10)
                .ToList();

            return Ok(new
            {
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Phone,
                    photo = BuildUserPhotoUrl(user.Photo)
                },
                totals = new
                {
                    tickets = _context.Tickets.Count(t => t.UserId == userId),
                    payments = _context.Payments.Count(p => p.UserId == userId),
                    complaints = _context.Complaints.Count(c => c.UserId == userId)
                },
                recentActions
            });
        }
    }
}