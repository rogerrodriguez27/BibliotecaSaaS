using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Socio>>> GetSocios()
        {
            return await _context.Socios.ToListAsync();
        }

        // POST: api/members
        [HttpPost]
        public async Task<ActionResult<Socio>> CreateSocio(Socio socio)
        {
            // Validación básica
            if (socio.InquilinoId <= 0)
                return BadRequest("Falta el InquilinoId");

            if (string.IsNullOrEmpty(socio.Codigo))
                return BadRequest("El socio debe tener un código (DNI/Matrícula)");

            _context.Socios.Add(socio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSocios), new { id = socio.SocioId }, socio);
        }
    }
}