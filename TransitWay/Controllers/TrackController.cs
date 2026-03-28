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

            if (minDistanceMeters > 50) // 50 meters tolerance
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

        
        //[HttpGet("eta/{busId}")]
        //public IActionResult GetETA(int busId)
        //{
        //    var bus = _context.Buses
        //        .FirstOrDefault(b => b.Id == busId);

        //    if (bus == null)
        //        return NotFound("Bus not found");

        //    var lastLocation = _context.BusLocations
        //        .Where(l => l.BusId == bus.Id)
        //        .OrderByDescending(l => l.LastUpdatedAt)
        //        .FirstOrDefault();

        //    if (lastLocation == null)
        //        return NotFound("No location data");

        //    var routePoints = _context.RoutePoints
        //        .Where(r => r.RouteId == bus.RouteId)
        //        .OrderBy(r => r.Id)
        //        .ToList();

        //    if (!routePoints.Any())
        //        return NotFound("No route points");

        //    int nearestIndex = 0;
        //    double minDistanceMeters = double.MaxValue;

        //    for (int i = 0; i < routePoints.Count; i++)
        //    {
        //        double dist = CalculateDistanceMeters(
        //            lastLocation.Latitude,
        //            lastLocation.Longitude,
        //            routePoints[i].Latitude,
        //            routePoints[i].Longitude);

        //        if (dist < minDistanceMeters)
        //        {
        //            minDistanceMeters = dist;
        //            nearestIndex = i;
        //        }
        //    }

        //    double remainingDistanceMeters = 0;

        //    for (int i = nearestIndex; i < routePoints.Count - 1; i++)
        //    {
        //        remainingDistanceMeters += CalculateDistanceMeters(
        //            routePoints[i].Latitude,
        //            routePoints[i].Longitude,
        //            routePoints[i + 1].Latitude,
        //            routePoints[i + 1].Longitude);
        //    }

        //    double remainingDistanceKm = remainingDistanceMeters / 1000.0;

        //    double speedKmPerHour = lastLocation.Speed > 0 ? lastLocation.Speed : 30;

        //    double etaHoursDecimal = remainingDistanceKm / speedKmPerHour;

        //    int totalMinutes = (int)Math.Ceiling(etaHoursDecimal * 60);

        //    if (totalMinutes < 1)
        //        totalMinutes = 1;

        //    int hours = totalMinutes / 60;
        //    int minutes = totalMinutes % 60;

        //    string etaFormatted;

        //    if (hours > 0)
        //        etaFormatted = $"{hours} hr {minutes} min";
        //    else
        //        etaFormatted = $"{minutes} min";

        //    return Ok(new
        //    {
        //        busId = bus.Id,
        //        remainingDistanceKm = Math.Round(remainingDistanceKm, 2),
        //        eta = etaFormatted
        //    });
        //}

     
        private double CalculateDistanceMeters(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            var R = 6371000; // meters
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