using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;

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
        public IActionResult GenerateRouteQr(int routeId , int BusId)
        {
            var route = _context.Routes.Find(routeId);

            if (route == null)
                return NotFound("Route not found");

            string token = GenerateSecureToken();

            var routeQr = new RouteQr
            {
                RouteId = routeId,
                BusId = BusId,
                Token = token
            };

            _context.RouteQrs.Add(routeQr);
            _context.SaveChanges();

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);

            byte[] qrImage = qrCode.GetGraphic(20);

            string qrbase64 = Convert.ToBase64String(qrImage);
            return Ok(new
            {
                Message = "Qr Generated Successfully",
                routeId = routeId,
                busId = BusId,
                token = token,
                qrImage = $"data:image/png;base64,{qrbase64}"
            });

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

            var route = _context.Routes.Include(r => r.Zone)
                .FirstOrDefault(r => r.Id == routeQr.RouteId);

            if (route.Zone == null)
                return BadRequest("Zone not configured");

            if (route == null)
                return BadRequest("Route not found");

            decimal fare = route.Zone.Price;

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserId == dto.UserId);

            if (wallet == null || wallet.Balance < fare)
                return BadRequest("Insufficient balance");

            wallet.Balance -= fare;

            int busId = routeQr.BusId;

            var ticket = new Ticket
            {
                UserId = dto.UserId,
                RouteId = route.Id,
                BusId = busId,
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
                busId = busId,
                route = route.Name,
                time = ticket.CreatedAt.ToLocalTime(),
                remainingBalance = wallet.Balance
            });
        }

        [HttpGet("qr-image/{token}")]
        public IActionResult GetQrImage(string token)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);

            var qrImage = qrCode.GetGraphic(20);

            return File(qrImage, "image/png");
        }
        private string GenerateSecureToken()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
    }
}