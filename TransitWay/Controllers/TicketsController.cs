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

      
        [HttpGet("user/{userId}")]
        public IActionResult GetUserTickets(int userId)
        {
            var tickets = _context.Tickets
                .Where(t => t.UserId == userId)
                .Include(t => t.Route)
                .Include(t => t.Bus)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    ticketId = t.Id,
                    route = t.Route.Name,
                    bus = t.Bus.PlateNumber,
                    price = t.Price,
                    time = t.CreatedAt.ToString("hh:mm tt"),
                    date = t.CreatedAt.ToString("dd-MM-yyyy"),
                    status = t.Status.ToString()
                })
                .ToList();

            return Ok(tickets);
        }

     
        [HttpGet]
        public IActionResult GetAllTickets()
        {
            var tickets = _context.Tickets
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Select(t => new TicketResponseDto
                {
                    Id = t.Id,
                    RouteName = t.Route.Name,
                    BusPlate = t.Bus.PlateNumber,
                    Price = t.Price,
                    Status = t.Status.ToString(),
                    CreatedAt = t.CreatedAt,
                    ExpireAt = t.ExpireAt,
                    IsUsed = t.IsUsed
                })
                .ToList();

            return Ok(tickets);
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
                QRToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(input.ValidHours),
                Status = TicketStatus.Valid,
                IsUsed = false
            };

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Ticket created successfully",
                ticketId = ticket.Id,
                qrToken = ticket.QRToken
            });
        }

       
        [HttpPost("use/{qrToken}")]
        public IActionResult UseTicket(string qrToken)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(t => t.QRToken == qrToken);

            if (ticket == null)
                return NotFound("Ticket not found");

            if (ticket.Status == TicketStatus.Used)
                return BadRequest("Already used");

            if (ticket.Status == TicketStatus.Cancelled)
                return BadRequest("Ticket cancelled");

            if (ticket.ExpireAt < DateTime.UtcNow)
            {
                ticket.Status = TicketStatus.Expired;
                _context.SaveChanges();
                return BadRequest("Ticket expired");
            }

            ticket.IsUsed = true;
            ticket.Status = TicketStatus.Used;
            ticket.UsedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok("Ticket used successfully");
        }

       
        
        [HttpPut("cancel/{id}")]
        public IActionResult CancelTicket(int id)
        {
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound("Ticket not found");

            if (ticket.Status == TicketStatus.Used)
                return BadRequest("Cannot cancel used ticket");

            ticket.Status = TicketStatus.Cancelled;

            _context.SaveChanges();

            return Ok("Ticket cancelled");
        }

       
        [HttpPut("status/{id}")]
        public IActionResult UpdateStatus(int id, [FromQuery] string status)
        {
            var ticket = _context.Tickets.Find(id);

            if (ticket == null)
                return NotFound("Ticket not found");

            if (!Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
                return BadRequest("Invalid status");

            ticket.Status = parsedStatus;

            _context.SaveChanges();

            return Ok("Status updated");
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