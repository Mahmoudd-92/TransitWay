using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
        private readonly HttpClient _httpClient;

        public UserTripController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchTrip(UserTripRequestDto request)
        {
            var startStation = _context.Stations.FirstOrDefault(s => s.Id == request.StartStationId);
            var endStation = _context.Stations.FirstOrDefault(s => s.Id == request.EndStationId);

            if (startStation == null || endStation == null)
                return NotFound("Invalid station");

            if (startStation.RouteId != endStation.RouteId)
                return BadRequest("Stations are not on same route");

            var routeId = startStation.RouteId;

            var buses = _context.Buses.Where(b => b.RouteId == routeId).ToList();

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

                double distanceMeters = await GetDistanceFromOrsm(
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

            double tripDistanceMeters = await GetDistanceFromOrsm(
                startStation.Latitude,
                startStation.Longitude,
                endStation.Latitude,
                endStation.Longitude
            );

            double tripDistanceKm = tripDistanceMeters / 1000.0;

            double speedKmPerHour = selectedLocation.Speed > 0 ? selectedLocation.Speed : 30;

            double etaHours = distanceToStationKm / speedKmPerHour;
            int totalMinutes = (int)Math.Ceiling(etaHours * 60);

            if (totalMinutes < 1)
                totalMinutes = 1;

            string etaFormatted = totalMinutes < 60
                ? $"{totalMinutes} min"
                : $"{totalMinutes / 60} hr {totalMinutes % 60} min";

            return Ok(new
            {
                Id = selectedBus.Id,
                BusNumber = selectedBus.BusNumber,
                DistanceToStationKm = $"{Math.Round(distanceToStationKm, 2)} km",
                TripDistanceKm = $"{Math.Round(tripDistanceKm, 2)} km",
                EstimatedArrivalTime = etaFormatted
            });
        }

     
        private async Task<double> GetDistanceFromOrsm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            string url =
                $"http://router.project-osrm.org/route/v1/driving/" +
                $"{lon1},{lat1};{lon2},{lat2}?overview=false";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return 0;

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var routes = doc.RootElement.GetProperty("routes");

            if (routes.GetArrayLength() == 0)
                return 0;

            return routes[0].GetProperty("distance").GetDouble();
        }
    }
}