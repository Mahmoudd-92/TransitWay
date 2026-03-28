using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTripController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserTripController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("search")]
        public IActionResult SearchTrip(UserTripRequestDto request)
        {
            var startStation = _context.Stations
                .FirstOrDefault(s => s.Id == request.StartStationId);

            var endStation = _context.Stations
                .FirstOrDefault(s => s.Id == request.EndStationId);

            if (startStation == null || endStation == null)
                return NotFound("Invalid station");

            if (startStation.RouteId != endStation.RouteId)
                return BadRequest("Stations are not on same route");

            var routeId = startStation.RouteId;

            var buses = _context.Buses
                .Where(b => b.RouteId == routeId)
                .ToList();

            if (!buses.Any())
                return NotFound("No buses available");

            Bus? selectedBus = null;
            BusLocation? selectedLocation = null;
            double minDistanceMeters = double.MaxValue;

            foreach (var bus in buses)
            {
                var lastLocation = _context.BusLocations
                    .Where(bl => bl.BusId == bus.Id)
                    .OrderByDescending(bl => bl.LastUpdatedAt)
                    .FirstOrDefault();

                if (lastLocation == null)
                    continue;

                double distanceMeters = CalculateDistanceMeters(
                    lastLocation.Latitude,
                    lastLocation.Longitude,
                    startStation.Latitude,
                    startStation.Longitude
                );

                if (distanceMeters < minDistanceMeters)
                {
                    minDistanceMeters = distanceMeters;
                    selectedBus = bus;
                    selectedLocation = lastLocation;
                }
            }

            if (selectedBus == null || selectedLocation == null)
                return NotFound("No active buses found");

            double distanceToStationKm = minDistanceMeters / 1000.0;

            var routePoints = _context.RoutePoints
                .Where(r => r.RouteId == routeId)
                .OrderBy(r => r.Order)
                .ToList();

            if (!routePoints.Any())
                return NotFound("Route points not found");

            int startIndex = GetClosestRoutePointIndex(routePoints, startStation);
            int endIndex = GetClosestRoutePointIndex(routePoints, endStation);

            if (startIndex > endIndex)
            {
                var temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }

            double tripDistanceMeters = 0;

            for (int i = startIndex; i < endIndex; i++)
            {
                tripDistanceMeters += CalculateDistanceMeters(
                    routePoints[i].Latitude,
                    routePoints[i].Longitude,
                    routePoints[i + 1].Latitude,
                    routePoints[i + 1].Longitude
                );
            }

            double tripDistanceKm = tripDistanceMeters / 1000.0;

            double speedKmPerHour = selectedLocation.Speed > 0
                ? selectedLocation.Speed
                : 30;

            double etaHoursDecimal = distanceToStationKm / speedKmPerHour;
            int totalMinutes = (int)Math.Ceiling(etaHoursDecimal * 60);

            if (totalMinutes < 1)
                totalMinutes = 1;

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            string etaFormatted = hours > 0
                ? $"{hours} hr {minutes} min"
                : $"{minutes} min";

            var response = new UserTripResponseDto
            {
                BusNumber = selectedBus.BusNumber,
                DistanceToStationKm = $"{Math.Round(distanceToStationKm, 2)} km",
                TripDistanceKm = $"{Math.Round(tripDistanceKm, 2)} km",
                EstimatedArrivalTime = etaFormatted
            };

            return Ok(response);
        }

        private int GetClosestRoutePointIndex(List<RoutePoint> points, Station station)
        {
            double minDistance = double.MaxValue;
            int index = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double distance = CalculateDistanceMeters(
                    points[i].Latitude,
                    points[i].Longitude,
                    station.Latitude,
                    station.Longitude
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    index = i;
                }
            }

            return index;
        }

        private double CalculateDistanceMeters(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            double R = 6371000;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}