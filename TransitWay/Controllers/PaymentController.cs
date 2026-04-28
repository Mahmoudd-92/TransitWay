using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;
using TransitWay.Dtos;
using TransitWay.Entites;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("vodafone-cash")]
        public IActionResult VodafoneCash([FromBody] WalletChargeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                !dto.PhoneNumber.StartsWith("01") ||
                dto.PhoneNumber.Length != 11)
                return BadRequest(new { message = "Invalid Vodafone Cash phone number" });

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            if (dto.Amount < 5)
                return BadRequest(new { message = "Minimum charge amount is 5 EGP" });

            if (dto.Amount > 5000)
                return BadRequest(new { message = "Maximum charge amount is 5000 EGP" });

            var user = _context.Users.Find(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == dto.UserId);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = dto.UserId,
                    Balance = 0
                };
                _context.Wallets.Add(wallet);
            }

            wallet.Balance += dto.Amount;

            _context.Payments.Add(new Payment
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                PaymentMethod = "Vodafone Cash",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            return Ok(new
            {
                message = "Wallet charged successfully via Vodafone Cash",
                phoneNumber = dto.PhoneNumber,
                amountCharged = dto.Amount,
                newBalance = wallet.Balance
            });
        }


        [HttpPost("instapay")]
        public IActionResult InstaPay([FromBody] WalletChargeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
                !dto.PhoneNumber.StartsWith("01") ||
                dto.PhoneNumber.Length != 11)
                return BadRequest(new { message = "Invalid InstaPay phone number" });

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            if (dto.Amount < 5)
                return BadRequest(new { message = "Minimum charge amount is 5 EGP" });

            if (dto.Amount > 10000)
                return BadRequest(new { message = "Maximum charge amount is 10000 EGP" });

            var user = _context.Users.Find(dto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == dto.UserId);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = dto.UserId,
                    Balance = 0
                };
                _context.Wallets.Add(wallet);
            }

            wallet.Balance += dto.Amount;

            _context.Payments.Add(new Payment
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                PaymentMethod = "InstaPay",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            return Ok(new
            {
                message = "Wallet charged successfully via InstaPay",
                phoneNumber = dto.PhoneNumber,
                amountCharged = dto.Amount,
                newBalance = wallet.Balance
            });
        }


        [HttpGet("history/{userId}")]
        public IActionResult GetPaymentHistory(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            var payments = _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    id = p.Id,
                    amount = p.Amount,
                    paymentMethod = p.PaymentMethod,
                    status = p.Status,
                    createdAt = p.CreatedAt
                })
                .ToList();

            return Ok(new
            {
                userId = userId,
                currentBalance = wallet?.Balance ?? 0,
                totalPayments = payments.Count,
                totalCharged = payments
                    .Where(p => p.status == "Success")
                    .Sum(p => p.amount),
                payments = payments
            });
        }
    }
}