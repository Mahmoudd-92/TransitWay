using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Hubs;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/track")]
    public class TrackController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TrackingHub> _hub;

        private const int WorkStartHour = 6;
        private const int WorkEndHour = 24;

        public TrackController(
            ApplicationDbContext context,
            IHubContext<TrackingHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] BusLocationDto loc)
        {
            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.Id == loc.BusId);

            if (bus == null)
                return NotFound("Bus not found");

            var location = new BusLocation
            {
                BusId = bus.Id,
                Latitude = loc.Lat,
                Longitude = loc.Lng,
                Speed = loc.speed,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.BusLocations.Add(location);
            await _context.SaveChangesAsync();

            CheckOffRoute(bus, loc);

            await _hub.Clients.All.SendAsync(
                "ReceiveLocationUpdate",
                bus.Id,
                loc.Lat,
                loc.Lng,
                loc.speed
            );

            return Ok(new { message = "Location updated successfully" });
        }

        private void CheckOffRoute(Bus bus, BusLocationDto loc)
        {
            var routePoints = _context.RoutePoints
                .Where(r => r.RouteId == bus.RouteId)
                .ToList();

            if (!routePoints.Any())
                return;

            double minDistanceMeters = double.MaxValue;

            foreach (var point in routePoints)
            {
                double dist = CalculateDistanceMeters(
                    loc.Lat,
                    loc.Lng,
                    point.Latitude,
                    point.Longitude);

                if (dist < minDistanceMeters)
                    minDistanceMeters = dist;
            }

            if (minDistanceMeters > 50)
            {
                CreateAlert(bus, "OffRoute");
            }
        }

        private void CreateAlert(Bus bus, string type)
        {
            var recentAlert = _context.Alerts
                .Where(a => a.BusId == bus.Id && a.Type == type)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            if (recentAlert != null &&
                (DateTime.UtcNow - recentAlert.CreatedAt).TotalMinutes < 5)
                return;

            var alert = new Alert
            {
                BusId = bus.Id,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _context.Alerts.Add(alert);
            _context.SaveChanges();

            _hub.Clients.All.SendAsync(
                "ReceiveAlert",
                bus.Id,
                type);
        }

        [HttpPost("start-trip/{busId}")]
        public IActionResult StartTrip(int busId)
        {
            var egyptTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time")
            );

            int currentHour = egyptTime.Hour;

            if (currentHour < WorkStartHour || currentHour >= WorkEndHour)
            {
                return BadRequest(new
                {
                    message = "Cannot start a trip outside working hours. Working hours are from 6:00 AM to 12:00 AM.",
                    currentTime = egyptTime.ToString("hh:mm tt"),
                    workingHours = "6:00 AM - 12:00 AM"
                });
            }

            var bus = _context.Buses
                .Include(b => b.Route)
                .FirstOrDefault(b => b.Id == busId);

            if (bus == null)
                return NotFound("Bus not found");

            if (bus.RouteId == null)
                return BadRequest("Bus has no route assigned");

            var driver = _context.Drivers
                .FirstOrDefault(d => d.BusId == busId);

            if (driver == null)
                return BadRequest(new { message = "No driver assigned to this bus" });

            if (driver.Status != "Active")
                return BadRequest(new { message = "Driver is inactive. Only active drivers can start a trip." });

            var existing = _context.Trips
                .FirstOrDefault(t => t.BusId == busId && !t.IsCompleted);
            if (existing != null)
                return BadRequest("Trip already in progress");

            var trip = new Trip
            {
                BusId = busId,
                RouteId = bus.RouteId,
                StartTime = DateTime.UtcNow,
                IsCompleted = false
            };
            _context.Trips.Add(trip);

            var tickets = _context.Tickets
                .Where(t => t.BusId == busId && t.TripStartTime == null)
                .ToList();
            foreach (var ticket in tickets)
                ticket.TripStartTime = DateTime.UtcNow;

            _context.SaveChanges();
            return Ok(new { message = "Trip started successfully", tripId = trip.Id });
        }

        [HttpPost("end-trip/{busId}")]
        public IActionResult EndTrip(int busId)
        {
            var trip = _context.Trips
                .FirstOrDefault(t => t.BusId == busId && !t.IsCompleted);
            if (trip == null)
                return BadRequest("No active trip found");

            trip.EndTime = DateTime.UtcNow;
            trip.IsCompleted = true;

            var tickets = _context.Tickets
                .Where(t => t.BusId == busId
                       && t.TripEndTime == null
                       && t.Status != TicketStatus.Expired
                       && t.Status != TicketStatus.Cancelled)
                .ToList();

            foreach (var ticket in tickets)
            {
                ticket.TripStartTime ??= trip.StartTime;
                ticket.TripEndTime = DateTime.UtcNow;
                ticket.Status = TicketStatus.Expired;
                ticket.UsedAt = DateTime.UtcNow;
            }

            _context.SaveChanges();
            return Ok(new { message = "Trip ended successfully", completedTickets = tickets.Count });
        }

        private double CalculateDistanceMeters(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            var R = 6371000;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}