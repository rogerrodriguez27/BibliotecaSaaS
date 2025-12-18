using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InquilinosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InquilinosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/inquilinos (Ver clientes existentes)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inquilino>>> GetInquilinos()
        {
            return await _context.Inquilinos.ToListAsync();
        }

        // POST: api/inquilinos (ONBOARDING: Registrar nueva Biblioteca Cliente)
        // Este endpoint debería ser público o protegido con una "SuperClave", 
        // lo dejaremos público para facilitar tus pruebas.
        [HttpPost]
        public async Task<ActionResult<Inquilino>> CreateInquilino(Inquilino inquilino)
        {
            // Validar unicidad del código (Ej: no pueden haber dos "USC2024")
            if (await _context.Inquilinos.AnyAsync(i => i.Codigo == inquilino.Codigo))
            {
                return BadRequest("El código de inquilino ya existe.");
            }

            inquilino.FechaCreacion = DateTime.UtcNow;
            inquilino.EstaActivo = true;

            _context.Inquilinos.Add(inquilino);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInquilinos), new { id = inquilino.InquilinoId }, inquilino);
        }
    }
}