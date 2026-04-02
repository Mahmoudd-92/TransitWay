using TransitWay.Dtos;
using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BusController(ApplicationDbContext context)
        {
            _context = context;
        }

      
        [HttpGet]
        public IActionResult GetAllBuses()
        {
            var buses = _context.Buses
                .Include(b => b.Driver)
                .Select(b => new BusResponseDto
                {
                    Id = b.Id,
                    PlateNumber = b.PlateNumber,
                    LicenseNumber = b.LicenseNumber,
                    BusNumber = b.BusNumber,
                    RouteId = b.RouteId,
                    Capacity = b.Capacity,
                    Status = b.Status,
                    DriverId = b.Id,
                    DriverName = b.Driver != null ? b.Driver.Name : null
                })
                .ToList();

            return Ok(buses);
        }

      
        [HttpPost]
        public IActionResult CreateBus(CreateBusDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bus = new Bus
            {
                BusNumber = input.BusNumber,
                PlateNumber = input.PlateNumber,
                LicenseNumber = input.LicenseNumber,
                Capacity = input.Capacity,
                RouteId =(int)input.RouteId,
                Status = "Active"
            };

            _context.Buses.Add(bus);
            _context.SaveChanges();

            return Ok(new { message = "Bus added successfully" });
        }

      
        [HttpDelete("{id}")]
        public IActionResult DeleteBus(int id)
        {
            var bus = _context.Buses.FirstOrDefault(b => b.Id == id);

            if (bus == null)
                return NotFound("Bus not found");

            _context.Buses.Remove(bus);
            _context.SaveChanges();

            return Ok(new { message = "Bus deleted successfully" });
        }

      
        [HttpPut("status/{id}")]
        public IActionResult ChangeStatus(int id)
        {
            var bus = _context.Buses.FirstOrDefault(b => b.Id == id);

            if (bus == null)
                return NotFound("Bus not found");

            bus.Status = bus.Status == "Active"
                ? "Inactive"
                : "Active";

            _context.SaveChanges();

            return Ok(new { message = "Status updated", bus.Status });
        }


        [HttpPost("assign-driver")]
        public IActionResult AssignDriver(int busId, int driverId)
        {
            var bus = _context.Buses.FirstOrDefault(b => b.Id == busId);
            var driver = _context.Drivers.FirstOrDefault(d => d.Id == driverId);

            if (bus == null || driver == null)
                return NotFound("Bus or Driver not found");

            driver.BusId = bus.Id;

            _context.SaveChanges();

            return Ok(new { message = "Driver assigned successfully" });
        }

     
        [HttpPost("unassign-driver/{busId}")]
        public IActionResult UnassignDriver(int busId)
        {
            var driver = _context.Drivers
                .FirstOrDefault(d => d.BusId == busId);

            if (driver == null)
                return NotFound("No driver assigned");

            driver.BusId = 0;

            _context.SaveChanges();

            return Ok(new { message = "Driver unassigned successfully" });
        }
    }
}