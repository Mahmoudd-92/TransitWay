using TransitWay.Entites;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using TransitWay.Data;
using TransitWay.Dtos;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrPaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QrPaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpPost("generate-route/{routeId}")]
        public IActionResult GenerateRouteQr(int routeId)
        {
            var route = _context.Routes.Find(routeId);

            if (route == null)
                return NotFound("Route not found");

            string token = GenerateSecureToken();

            var routeQr = new RouteQr
            {
                RouteId = routeId,
                Token = token
            };

            _context.RouteQrs.Add(routeQr);
            _context.SaveChanges();

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);

            byte[] qrImage = qrCode.GetGraphic(20);

            return File(qrImage, "image/png");
        }

      
        [HttpPost("scan-pay")]
        public IActionResult ScanAndPay([FromBody] ScanRouteDto dto)
        {
            var user = _context.Users.Find(dto.UserId);

            if (user == null)
                return BadRequest("User not found");

            var routeQr = _context.RouteQrs
                .FirstOrDefault(r => r.Token == dto.QrText);

            if (routeQr == null)
                return BadRequest("Invalid QR");

            var route = _context.Routes.Find(routeQr.RouteId);

            decimal fare = 10.0m; 

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserId == dto.UserId);

            if (wallet == null || wallet.Balance < fare)
                return BadRequest("Insufficient balance");

            wallet.Balance -= fare;

            var ticket = new Ticket
            {
                UserId = dto.UserId,
                RouteId = route.Id,
                BusId = dto.BusId,
                Price = fare,
                QRToken = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(2),
                IsUsed = false,
                Status = TicketStatus.Valid
            };

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Fare paid successfully",
                ticketId = ticket.Id,
                bus = dto.BusId,
                time = ticket.CreatedAt,
                route = route.Name,
                remainingBalance = wallet.Balance
            });
        }
      
        private string GenerateSecureToken()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
    }
}