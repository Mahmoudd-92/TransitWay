using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Services.AttachmentService;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentService _attachmentService;

        public DriverController(ApplicationDbContext context, IAttachmentService attachmentService)
        {
            _context = context;
            _attachmentService = attachmentService;
        }

        private static string HashLegacyPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash))
                return false;

            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                    return true;
            }
            catch
            {
                // ignore invalid bcrypt hash format and fall back to legacy hash
            }

            return storedHash == HashLegacyPassword(password);
        }

        private string? BuildPhotoUrl(string? photoName)
        {
            if (string.IsNullOrWhiteSpace(photoName))
                return null;

            return $"{Request.Scheme}://{Request.Host}/images/drivers/{photoName}";
        }

        [HttpPost("login")]
        public IActionResult DriverLogin([FromBody] LoginDto input)
        {
            var driver = _context.Drivers
                .Include(d => d.Bus)
                .FirstOrDefault(u => u.Email == input.Email);

            if (driver == null || !VerifyPassword(input.Password, driver.PasswordHash))
                return Unauthorized("Invalid email or password");

            if (driver.PasswordHash == HashLegacyPassword(input.Password))
            {
                driver.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);
                _context.SaveChanges();
            }

            return Ok(new DriverResponseDto
            {
                Id = driver.Id,
                Name = driver.Name,
                Email = driver.Email,
                Phone = driver.Phone,
                Bus = driver.Bus?.BusNumber,
                BusId = driver.BusId,
                LicenseNumber = driver.LicenseNumber,
                Status = driver.Status,
                Photo = BuildPhotoUrl(driver.Photo)
            });
        }

        [HttpPost("get-email")]
        public IActionResult GetEmail(PhoneRequestDto input)
        {
            var driver = _context.Drivers.FirstOrDefault(d => d.Phone == input.PhoneNumber);
            if (driver == null) return NotFound("Phone Number Not Found");
            return Ok(new
            {
                email = driver.Email,
            });
        }


        [HttpGet]
        public IActionResult GetAllDrivers()
        {
            var drivers = _context.Drivers
                .Include(d => d.Bus)
                .ToList()
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Phone,
                    d.Email,
                    d.LicenseNumber,
                    d.Status,
                    Photo = BuildPhotoUrl(d.Photo),
                    Bus = d.Bus == null ? null : new
                    {
                        d.Bus.Id,
                        d.Bus.BusNumber
                    }
                })
                .ToList();

            return Ok(drivers);
        }



        [HttpGet("{id}")]
        public IActionResult GetDriverById(int id)
        {
            var driver = _context.Drivers
                .Include(d => d.Bus)
                .ThenInclude(b => b.Route)
                .ThenInclude(r => r.Stations)
                .FirstOrDefault(d => d.Id == id);

            if (driver == null)
                return NotFound("Driver not found");

            return Ok(new
            {
                driver.Id,
                driver.Name,
                driver.Phone,
                driver.Email,
                driver.LicenseNumber,
                driver.Status,
                Photo = BuildPhotoUrl(driver.Photo),
                Bus = driver.Bus == null ? null : new
                {
                    driver.Bus.Id,
                    driver.Bus.BusNumber,
                    driver.Bus.PlateNumber,
                    driver.Bus.RouteId,
                },
                RouteName = driver.Bus?.Route?.Name,    
                NumberOfStations = driver?.Bus?.Route?.Stations?.Count ?? 0
            });
        }



        [HttpPost]
        public IActionResult CreateDriver([FromForm] CreateDriverDto input)
        {
            if (_context.Drivers.Any(d => d.Email == input.Email))
                return BadRequest("Email already exists");

            if (_context.Drivers.Any(d => d.Phone == input.PhoneNumber))
                return BadRequest("Phone already exists");

            if (input.Photo == null || input.Photo.Length == 0)
                return BadRequest("Driver photo is required");

            var photoName = _attachmentService.Upload("drivers", input.Photo);
            if (string.IsNullOrWhiteSpace(photoName))
                return BadRequest("Invalid photo. Only jpg, jpeg, png up to 5MB are allowed.");

            var driver = new Driver
            {
                Name = input.FullName,
                Phone = input.PhoneNumber,
                Email = input.Email,
                LicenseNumber = input.LicenseNumber,
                Photo = photoName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password),
                Status = "Inactive"

            };

            _context.Drivers.Add(driver);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Driver created successfully",
                photo = $"/images/drivers/{photoName}"
            });
        }



        [HttpPut("{id}")]
        public IActionResult UpdateDriver(int id, [FromForm] UpdateDriverDto input)
        {
            var driver = _context.Drivers.Find(id);

            if (driver == null)
                return NotFound("Driver not found");

            if (_context.Drivers.Any(d => d.Id != id && d.Email == input.Email))
                return BadRequest("Email already exists");

            if (_context.Drivers.Any(d => d.Id != id && d.Phone == input.PhoneNumber))
                return BadRequest("Phone already exists");

            if (input.Photo != null && input.Photo.Length > 0)
            {
                var photoName = _attachmentService.Upload("drivers", input.Photo);
                if (string.IsNullOrWhiteSpace(photoName))
                    return BadRequest("Invalid photo. Only jpg, jpeg, png up to 5MB are allowed.");

                if (!string.IsNullOrWhiteSpace(driver.Photo))
                    _attachmentService.Delete(driver.Photo, "drivers");

                driver.Photo = photoName;
            }
            driver.Name = input.FullName;
            driver.Phone = input.PhoneNumber;
            driver.Email = input.Email;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Driver updated successfully",
                Photo = BuildPhotoUrl(driver.Photo)
            });
        }

        [HttpPost("sos")]
        public IActionResult SendSos([FromForm] CreateDriverSosDto dto)
        {
            var driver = _context.Drivers
                .Include(d => d.Bus)
                .FirstOrDefault(d => d.Id == dto.DriverId);

            if (driver == null)
                return NotFound("Driver not found");

            if (driver.BusId == null)
                return BadRequest("Driver is not assigned to a bus");

            string? photoPath = null;
            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                photoPath = _attachmentService.Upload("results", dto.Photo);
                if (string.IsNullOrWhiteSpace(photoPath))
                    return BadRequest("Invalid photo. Only jpg, jpeg, png up to 5MB are allowed.");
            }

            var alert = new Alert
            {
                BusId = driver.BusId.Value,
                Type = dto.SituationType,
                Description = dto.Description,
                SituationPhotoPath = photoPath,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                NeedReplacementBus = dto.NeedReplacementBus,
                IsSos = true,
                CreatedAt = DateTime.UtcNow,
                Status = SosAlertStatus.PendingAdminReview
            };

            _context.Alerts.Add(alert);
            _context.SaveChanges();

            return Ok(new
            {
                message = "SOS alert created and sent to admins dashboard",
                alertId = alert.Id,
                busId = alert.BusId,
                location = new { alert.Latitude, alert.Longitude },
                needReplacementBus = alert.NeedReplacementBus,
                status = alert.Status,
                photoUrl = string.IsNullOrWhiteSpace(alert.SituationPhotoPath)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}/uploads/results/{alert.SituationPhotoPath}"
            });
        }

        [HttpPost("sos/{alertId}/start-safety-check")]
        public IActionResult StartSafetyCheck(int alertId)
        {
            var alert = _context.Alerts.FirstOrDefault(a => a.Id == alertId && a.IsSos);
            if (alert == null)
                return NotFound("SOS alert not found");

            alert.SafetyCheckStartedAt = DateTime.UtcNow;
            alert.Status = SosAlertStatus.AwaitingDriverCheckResponse;
            _context.SaveChanges();

            return Ok(new
            {
                message = "Safety check started. Driver has 60 seconds to confirm they are okay.",
                alertId = alert.Id,
                startedAtUtc = alert.SafetyCheckStartedAt,
                timeoutSeconds = 60
            });
        }

        [HttpPost("sos/{alertId}/safety-check-response")]
        public IActionResult SubmitSafetyCheckResponse(int alertId, [FromBody] DriverSafetyCheckResponseDto dto)
        {
            var alert = _context.Alerts.FirstOrDefault(a => a.Id == alertId && a.IsSos);
            if (alert == null)
                return NotFound("SOS alert not found");

            if (alert.SafetyCheckStartedAt == null)
                return BadRequest("Safety check was not started for this SOS alert.");

            if (alert.Status == SosAlertStatus.EscalatedToAuthorities)
                return BadRequest("This SOS alert is already escalated to authorities.");

            if (alert.Status == SosAlertStatus.DriverConfirmedSafe)
                return BadRequest("Driver safety response was already confirmed.");

            var elapsedSeconds = (DateTime.UtcNow - alert.SafetyCheckStartedAt.Value).TotalSeconds;
            if (elapsedSeconds > 60)
            {
                alert.DriverIsOkay = false;
                alert.Status = SosAlertStatus.EscalatedToAuthorities;
                _context.SaveChanges();

                return Ok(new
                {
                    message = "Safety window expired with late response. Escalated to authorities.",
                    alertId = alert.Id,
                    status = alert.Status,
                    emergencyPayload = new
                    {
                        alert.BusId,
                        alert.Type,
                        alert.Description,
                        alert.Latitude,
                        alert.Longitude,
                        alert.CreatedAt
                    }
                });
            }

            alert.DriverIsOkay = dto.IsOkay;

            if (dto.IsOkay)
            {
                alert.Status = SosAlertStatus.DriverConfirmedSafe;
                _context.SaveChanges();

                return Ok(new
                {
                    message = "Driver confirmed they are okay. No escalation triggered.",
                    alertId = alert.Id,
                    status = alert.Status
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.AdditionalDetails))
            {
                alert.Description = string.IsNullOrWhiteSpace(alert.Description)
                    ? dto.AdditionalDetails.Trim()
                    : $"{alert.Description}\n\nDriver follow-up: {dto.AdditionalDetails.Trim()}";
            }

            alert.Status = SosAlertStatus.EscalatedToAuthorities;
            _context.SaveChanges();

            return Ok(new
            {
                message = "Driver reported severe incident. Escalated to ambulance, fire truck, and authorities.",
                alertId = alert.Id,
                status = alert.Status,
                emergencyPayload = new
                {
                    alert.BusId,
                    alert.Type,
                    alert.Description,
                    alert.Latitude,
                    alert.Longitude,
                    alert.CreatedAt
                }
            });
        }

        [HttpPost("sos/{alertId}/auto-escalate-no-response")]
        public IActionResult AutoEscalateNoResponse(int alertId)
        {
            var alert = _context.Alerts.FirstOrDefault(a => a.Id == alertId && a.IsSos);
            if (alert == null)
                return NotFound("SOS alert not found");

            if (alert.DriverIsOkay == true)
                return BadRequest("Driver already confirmed they are safe.");

            alert.Status = SosAlertStatus.EscalatedToAuthorities;
            alert.DriverIsOkay = false;
            _context.SaveChanges();

            return Ok(new
            {
                message = "No response from driver after safety window. Escalation to authorities should be triggered by dispatcher integration.",
                alertId = alert.Id,
                status = alert.Status,
                emergencyPayload = new
                {
                    alert.BusId,
                    alert.Type,
                    alert.Description,
                    alert.Latitude,
                    alert.Longitude,
                    alert.CreatedAt
                }
            });
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteDriver(int id)
        {
            var driver = _context.Drivers.Find(id);

            if (driver == null)
                return NotFound("Driver not found");

            if (driver.BusId != null)
                return BadRequest("Driver is assigned to a bus");

            if (!string.IsNullOrWhiteSpace(driver.Photo))
                _attachmentService.Delete(driver.Photo, "drivers");

            _context.Drivers.Remove(driver);
            _context.SaveChanges();

            return Ok(new { message = "Driver deleted successfully" });
        }



        [HttpPut("status/{id}")]
        public IActionResult ChangeDriverStatus(int id, [FromQuery] string status)
        {
            var driver = _context.Drivers.Find(id);

            if (driver == null)
                return NotFound("Driver not found");

            driver.Status = status;

            _context.SaveChanges();

            return Ok(new { message = "Driver status updated" });
        }



        [HttpPost("assign")]
        public IActionResult AssignDriverToBus(int driverId, int busId)
        {
            var driver = _context.Drivers.Find(driverId);
            var bus = _context.Buses.Find(busId);

            if (driver == null || bus == null)
                return NotFound("Driver or Bus not found");

            if (driver.Status != "Active")
                return BadRequest("Driver is not active");

            if (_context.Drivers.Any(d => d.BusId == busId))
                return BadRequest("Bus already has a driver");

            driver.BusId = busId;

            _context.SaveChanges();

            return Ok(new { message = "Driver assigned to bus successfully" });
        }



        [HttpPost("unassign/{driverId}")]
        public IActionResult UnassignDriver(int driverId)
        {
            var driver = _context.Drivers.Find(driverId);

            if (driver == null)
                return NotFound("Driver not found");

            driver.BusId = null;

            _context.SaveChanges();

            return Ok(new { message = "Driver unassigned from bus" });
        }



        [HttpPut("reset-password/{id}")]
        public IActionResult ResetPassword(int id, [FromBody] string newPassword)
        {
            var driver = _context.Drivers.Find(id);

            if (driver == null)
                return NotFound("Driver not found");

            driver.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.SaveChanges();

            return Ok(new { message = "Password reset successfully" });
        }


        [HttpGet("search")]
        public IActionResult SearchDrivers([FromQuery] string name)
        {
            var drivers = _context.Drivers
                .Where(d => d.Name.Contains(name))
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Phone,
                    d.Email
                })
                .ToList();

            return Ok(drivers);
        }

        [HttpGet("details/{driverId}")]
        public IActionResult GetDriverDetails(int driverId)
        {
            var driver = _context.Drivers
                .Include(d => d.Bus)
                .ThenInclude(b => b.Route)
                .FirstOrDefault(d => d.Id == driverId);

            if (driver == null) return NotFound("Driver not found");
            if (driver.Bus == null) return BadRequest("Driver has no bus");

            var busId = driver.Bus.Id;

            var trips = _context.Trips
                .Where(t => t.BusId == busId)
                .ToList();

            int totalTrips = trips.Count;

            double totalHours = trips
                .Where(t => t.EndTime != null)
                .Sum(t => (t.EndTime.Value - t.StartTime).TotalHours);

            var completedTrips = trips.Where(t => t.EndTime != null).ToList();

            int onTimeTrips = completedTrips
                .Count(t => (t.EndTime.Value - t.StartTime).TotalMinutes <= 60);

            int onTimeRate = completedTrips.Any()
                ? (int)((double)onTimeTrips / completedTrips.Count * 100)
                : 0;

            var ratings = _context.driverRatings
                .Where(r => r.DriverId == driverId)
                .ToList();
            double rating = ratings.Any() ? ratings.Average(r => r.Rate) : 0;

            var weekly = trips
                .Where(t => t.StartTime >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(t => t.StartTime.DayOfWeek)
                .Select(g => new { day = g.Key.ToString(), count = g.Count() })
                .ToList();

            var recentTrips = trips
                .OrderByDescending(t => t.StartTime)
                .Take(5)
                .Select(t => new
                {
                    route = driver.Bus.Route?.Name ?? "Unknown",
                    date = t.StartTime.ToLocalTime().ToString("dd MMM yyyy"),
                    startTime = t.StartTime.ToLocalTime().ToString("hh:mm tt"),
                    endTime = t.EndTime != null
                                  ? t.EndTime.Value.ToLocalTime().ToString("hh:mm tt")
                                  : "Running",
                    duration = t.EndTime != null
                                  ? $"{(t.EndTime.Value - t.StartTime).TotalMinutes:F0} min"
                                  : "In Progress",
                    status = t.IsCompleted ? "Completed" : "Running"
                })
                .ToList();

            return Ok(new
            {
                driver = new
                {
                    driver.Id,
                    driver.Name,
                    driver.Email,
                    driver.Phone,
                    driver.LicenseNumber,
                    driver.Status,
                    Photo = BuildPhotoUrl(driver.Photo)
                },
                bus = new
                {
                    driver.Bus.BusNumber,
                    driver.Bus.PlateNumber,
                    route = driver.Bus.Route?.Name
                },
                stats = new
                {
                    totalTrips,
                    totalHours = Math.Round(totalHours, 1),
                    rating = Math.Round(rating, 1),
                    onTimeRate
                },
                weeklyActivity = weekly,
                recentTrips = recentTrips
            });
        }
        [HttpGet("profile/{id}")]
        public IActionResult GetDriverProfile(int id)
        {
            var driver = _context.Drivers
                .Include(d => d.Bus)
                .FirstOrDefault(d => d.Id == id);

            if (driver == null)
                return NotFound();

            return Ok(new
            {
                driver.Id,
                driver.Name,
                driver.Phone,
                driver.Email,
                driver.Status,
                Photo = BuildPhotoUrl(driver.Photo),
                BusNumber = driver.Bus?.BusNumber
            });
        }
    }
}