using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InquilinosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos la conexión a la BD
        public InquilinosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/inquilinos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inquilino>>> GetInquilinos()
        {
            // Esto va a SQL Server, hace un SELECT * FROM Inquilinos y te lo devuelve
            return await _context.Inquilinos.ToListAsync();
        }
    }
}
