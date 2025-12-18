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

        // PUT: api/members/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSocio(int id, Socio socio)
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            if (id != socio.SocioId) return BadRequest();

            var socioExistente = await _context.Socios
                .FirstOrDefaultAsync(s => s.SocioId == id && s.InquilinoId == inquilinoId);

            if (socioExistente == null) return NotFound();

            // Actualizar datos
            socioExistente.NombreCompleto = socio.NombreCompleto;
            socioExistente.Email = socio.Email;
            socioExistente.Codigo = socio.Codigo; // Cuidado con duplicados, pero por ahora lo permitimos editar
            socioExistente.TipoSocio = socio.TipoSocio;
            socioExistente.Estado = socio.Estado; // Para desactivarlo sin borrarlo

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("El código de socio ya existe en otro registro.");
            }

            return NoContent();
        }

        // DELETE: api/members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSocio(int id)
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.SocioId == id && s.InquilinoId == inquilinoId);

            if (socio == null) return NotFound();

            try
            {
                _context.Socios.Remove(socio);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest("No se puede eliminar al socio porque tiene préstamos históricos.");
            }
        }
    }
}