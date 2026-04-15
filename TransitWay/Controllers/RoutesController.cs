using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos.Routes;
using TransitWay.Entites;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllRoutes()
        {
            var routes = _context.Routes
                .Include(r => r.Zone)
                .Select(r => new RouteResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Zone = r.Zone.Name
                })
                .ToList();

            return Ok(routes);
        }

        [HttpPost]
        public IActionResult Create(CreateRouteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var zone = _context.Zones.Find(dto.ZoneId);
            if (zone == null)
                return BadRequest("Zone not found");

            // 🔥 هنا بنحوّل DTO → Entity
            var route = new TransitWay.Entites.Route
            {
                Name = dto.Name,
                ZoneId = dto.ZoneId,
            };

            _context.Routes.Add(route);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Route created successfully",
                routeId = route.Id,
                zone = zone.Name
            });
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRouteDto dto)
        {
            var route = _context.Routes.Find(id);

            if (route == null)
                return NotFound("Route not found");

            var zone = _context.Zones.Find(dto.ZoneId);
            if (zone == null)
                return BadRequest("Zone not found");

            route.Name = dto.Name;
            route.ZoneId = dto.ZoneId;
            _context.SaveChanges();

            return Ok("Route updated successfully");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var route = _context.Routes
                .Include(r => r.Stations)
                .FirstOrDefault(r => r.Id == id);

            if (route == null)
                return NotFound("Route not found");

            if (route.Stations.Any())
                return BadRequest("Cannot delete route with stations");

            _context.Routes.Remove(route);
            _context.SaveChanges();

            return Ok("Route deleted successfully");
        }
    }
}