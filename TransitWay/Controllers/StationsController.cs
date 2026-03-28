using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        [HttpGet]
        public IActionResult GetAllStations()
        {
            var stations = _context.Stations
                .Select(s => new StationResponseDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    Zone = s.Zone,
                    LatLong = $"{s.Latitude} & {s.Longitude}",
                    Status = s.Status
                })
                .ToList();

            return Ok(stations);
        }

        [HttpPost]
        public IActionResult CreateStation(CreateStationDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int count = _context.Stations.Count() + 1;
            string code = $"S-{count:D3}";

            var station = new Station
            {
                Code = code,
                Name = input.Name,
                Zone = input.Zone,
                Latitude = input.Latitude,
                Longitude = input.Longitude,
                Status = "Empty",
                RouteId = input.RouteId
            };

            _context.Stations.Add(station);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Station added successfully",
                stationId = station.Id
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStation(int id)
        {
            var station = _context.Stations.Find(id);

            if (station == null)
                return NotFound("Station not found");

            _context.Stations.Remove(station);
            _context.SaveChanges();

            return Ok("Station deleted");
        }

     
        [HttpPut("status/{id}")]
        public IActionResult UpdateStatus(int id, [FromQuery] string status)
        {
            var station = _context.Stations.Find(id);

            if (station == null)
                return NotFound("Station not found");

            station.Status = status;
            _context.SaveChanges();

            return Ok("Status updated");
        }
    }
}