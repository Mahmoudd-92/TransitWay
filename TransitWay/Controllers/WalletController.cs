using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : Controller
    {
     private readonly ApplicationDbContext _context;
        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("balance/{userId}")]
        public IActionResult GetBalance(int userId)
        {
            var wallet = _context.Wallets
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Id)
                .FirstOrDefault();

            if (wallet == null)
            {
                return Ok(new
                {
                    balancePoints = 0
                });
            }

            return Ok(new
            {
                balancePoints = wallet.Balance
            });
        }
    }
}
