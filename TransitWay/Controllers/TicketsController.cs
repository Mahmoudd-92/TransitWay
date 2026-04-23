using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitWay.Data;
using TransitWay.Dtos;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetTicketStatus(Ticket t)
        {
            if (t.Status == TicketStatus.Cancelled)
                return "Cancelled";

            if (t.ExpireAt < DateTime.UtcNow)
                return "Expired";

            return "Sold";
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserTickets(int userId)
        {
            var tickets = _context.Tickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Route)
                .Include(t => t.Bus)
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .Select(t => new
                {
                    ticketId = t.Id,
                    route = t.Route.Name,
                    BusNumber = t.Bus.BusNumber,
                    BusPlate = t.Bus.PlateNumber,
                    price = t.Price,
                    time = t.CreatedAt.ToLocalTime().ToString("hh:mm tt"),
                    date = t.CreatedAt.ToLocalTime().ToString("dd-MM-yyyy"),
                    status = GetTicketStatus(t)
                });

            return Ok(tickets);
        }

        [HttpGet("bus/{busId}")]
        public IActionResult GetbusTickets(int busId)
        {
            var tickets = _context.Tickets
                .Where(t => t.BusId == busId)
                .Include(t => t.Route)
                .Include(t => t.Bus)
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .Select(t => new
                {
                    ticketId = t.Id,
                    route = t.Route.Name,
                    BusNumber = t.Bus.BusNumber,
                    BusPlate = t.Bus.PlateNumber,
                    price = t.Price,
                    time = t.CreatedAt.ToLocalTime().ToString("hh:mm tt"),
                    date = t.CreatedAt.ToLocalTime().ToString("dd-MM-yyyy"),
                    status = GetTicketStatus(t)
                });

            return Ok(tickets);
        }

        [HttpGet]
        public IActionResult GetAllTickets()
        {
            var tickets = _context.Tickets
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .OrderByDescending(t => t.CreatedAt)
                .ToList()
                .Select(t => new TicketResponseDto
                {
                    Id = t.Id,
                    PassengerID = t.UserId,
                    RouteName = t.Route.Name,
                    BusNumber = t.Bus.BusNumber,
                    BusPlate = t.Bus.PlateNumber,
                    Price = t.Price,
                    Status = GetTicketStatus(t),
                    CreatedAt = t.CreatedAt.ToLocalTime(),
                    ExpireAt = t.ExpireAt.ToLocalTime()
                });

            return Ok(tickets);
        }

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            var tickets = _context.Tickets.ToList();

            var total = tickets.Count;
            var sold = tickets.Count(t => GetTicketStatus(t) == "Sold");
            var cancelled = tickets.Count(t => GetTicketStatus(t) == "Cancelled");
            var expired = tickets.Count(t => GetTicketStatus(t) == "Expired");

            return Ok(new
            {
                total,
                sold,
                cancelled,
                expired
            });
        }

        [HttpPost]
        public IActionResult CreateTicket(CreateTicketDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bus = _context.Buses.Find(input.BusId);
            var user = _context.Users.Find(input.UserId);
            var route = _context.Routes.Find(input.RouteId);

            if (bus == null || user == null || route == null)
                return BadRequest("Invalid data");

            var ticket = new Ticket
            {
                UserId = input.UserId,
                BusId = input.BusId,
                RouteId = input.RouteId,
                Price = input.Price,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(input.ValidHours),

                Status = TicketStatus.Sold
            };

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Ticket created & sold successfully",
                ticketId = ticket.Id
            });
        }

        private User GetOrCreateManualPassengerUser()
        {
            const string manualEmail = "manual.passenger@transitway.local";

            var manualUser = _context.Users.FirstOrDefault(u => u.Email == manualEmail);

            if (manualUser != null)
                return manualUser;

            manualUser = new User
            {
                FullName = "Manual Passenger",
                Email = manualEmail,
                PasswordHash = "MANUAL_PASSENGER",
                Phone = null,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(manualUser);
            _context.SaveChanges();

            return manualUser;
        }

        [HttpPost("manual/by-driver")]
        public IActionResult CreateManualTicketsByDriver([FromBody] CreateManualTicketByDriverDto input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var driver = _context.Drivers
                .Include(d => d.Bus)
                .ThenInclude(b => b.Route)
                .ThenInclude(r => r.Zone)
                .FirstOrDefault(d => d.Id == input.DriverId);

            if (driver == null)
                return NotFound("Driver not found");

            if (driver.Bus == null)
                return BadRequest("Driver is not assigned to a bus");

            if (driver.Bus.Id != input.BusId)
                return BadRequest("Bus does not belong to this driver");

            var passenger = GetOrCreateManualPassengerUser();

            var route = driver.Bus.Route;

            if (route == null)
                return BadRequest("Bus route not found");

            var ticketPrice = input.Price ?? route.Zone?.Price;

            if (ticketPrice == null || ticketPrice <= 0)
                return BadRequest("Ticket price is invalid");

            var now = DateTime.UtcNow;
            var expireAt = now.AddHours(input.ValidHours);

            var tickets = Enumerable.Range(0, input.NumberOfTickets)
                .Select(_ => new Ticket
                {
                    UserId = passenger.Id,
                    BusId = driver.Bus.Id,
                    RouteId = route.Id,
                    Price = ticketPrice.Value,
                    QRToken = Guid.NewGuid().ToString("N"),
                    CreatedAt = now,
                    ExpireAt = expireAt,
                    IsUsed = false,
                    Status = TicketStatus.Sold
                })
                .ToList();

            _context.Tickets.AddRange(tickets);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Manual tickets created successfully",
                driverId = driver.Id,
                busId = driver.Bus.Id,
                routeId = route.Id,
                numberOfTickets = tickets.Count,
                pricePerTicket = ticketPrice.Value,
                totalAmount = ticketPrice.Value * tickets.Count,
                ticketIds = tickets.Select(t => t.Id).ToList()
            });
        }

        [HttpPut("cancel/{id}")]
        public IActionResult CancelTicket(int id)
        {
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound("Ticket not found");

            ticket.Status = TicketStatus.Cancelled;

            _context.SaveChanges();

            return Ok("Ticket cancelled");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTicket(int id)
        {
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound("Ticket not found");

            _context.Tickets.Remove(ticket);
            _context.SaveChanges();

            return Ok("Ticket deleted");
        }
    }
}