using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CopiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CopiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/copies (Ver todas las copias y a qué libro pertenecen)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ejemplar>>> GetEjemplares()
        {
            // El .Include(e => e.Libro) sirve para que traiga también el Título del libro, no solo el ID
            return await _context.Ejemplares
                                 .Include(e => e.Libro)
                                 .ToListAsync();
        }

        // POST: api/copies (Registrar una copia física nueva)
        [HttpPost]
        public async Task<ActionResult<Ejemplar>> CreateEjemplar(Ejemplar ejemplar)
        {
            if (ejemplar.LibroId <= 0 || ejemplar.InquilinoId <= 0)
                return BadRequest("Datos incompletos (LibroId o InquilinoId)");

            // Validar que el código de barras no exista ya (Regla de negocio)
            bool existe = await _context.Ejemplares
                .AnyAsync(e => e.CodigoBarras == ejemplar.CodigoBarras && e.InquilinoId == ejemplar.InquilinoId);

            if (existe)
                return BadRequest("Ya existe una copia con ese Código de Barras en esta biblioteca.");

            // Estado inicial siempre disponible
            ejemplar.Estado = "Disponible";

            _context.Ejemplares.Add(ejemplar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEjemplares), new { id = ejemplar.EjemplarId }, ejemplar);
        }
    }
}