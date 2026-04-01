using Microsoft.AspNetCore.Mvc;
using TransitWay.Data;

namespace TransitWay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : Controller
    {

        private readonly ApplicationDbContext _context;

        public RoutesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAllRoutes()
        {
            var routes = _context.Routes.Select(r => new
            {
                id = r.Id,
                name = r.Name
            }).ToList();
            return Ok(routes);
        }

    }
}
