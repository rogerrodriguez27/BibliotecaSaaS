using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/loans (SOLICITAR PRÉSTAMO)
        [HttpPost]
        public async Task<ActionResult<Prestamo>> CreatePrestamo(Prestamo prestamo)
        {
            // 1. Validar datos mínimos
            if (prestamo.InquilinoId <= 0 || prestamo.EjemplarId <= 0 || prestamo.SocioId <= 0)
                return BadRequest("Faltan datos (Inquilino, Ejemplar o Socio).");

            // 2. BUSCAR EL EJEMPLAR Y VERIFICAR DISPONIBILIDAD
            var ejemplar = await _context.Ejemplares
                .FirstOrDefaultAsync(e => e.EjemplarId == prestamo.EjemplarId && e.InquilinoId == prestamo.InquilinoId);

            if (ejemplar == null)
                return NotFound("El ejemplar no existe.");

            if (ejemplar.Estado != "Disponible")
                return BadRequest($"El ejemplar no está disponible (Estado actual: {ejemplar.Estado}).");

            // 3. BUSCAR AL SOCIO Y VERIFICAR ESTADO
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.SocioId == prestamo.SocioId && s.InquilinoId == prestamo.InquilinoId);

            if (socio == null)
                return NotFound("El socio no existe.");

            if (socio.Estado != "Activo")
                return BadRequest("El socio no está activo y no puede pedir libros.");

            // 4. APLICAR REGLAS DE NEGOCIO (Crear préstamo + Bloquear libro)

            // Calculamos fecha de vencimiento (Ej: 14 días a partir de hoy)
            prestamo.FechaPrestamo = DateTime.UtcNow;
            prestamo.FechaVencimiento = DateTime.UtcNow.AddDays(14);
            prestamo.Estado = "Activo";

            // === AGREGAR ESTO (Parche temporal) ===
            // Como no hay login, usamos el ID 1 (Admin) que creó el script SQL por defecto
            prestamo.RegistradoPorUsuarioId = 1;
            // ======================================

            // Guardamos el préstamo
            _context.Prestamos.Add(prestamo);

            // CAMBIAMOS EL ESTADO DEL LIBRO A "PRESTADO"
            ejemplar.Estado = "Prestado";
            // (Entity Framework detecta que modificamos 'ejemplar' y generará el UPDATE automático)

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrestamo), new { id = prestamo.PrestamoId }, prestamo);
        }

        // GET: api/loans/{id} (Para ver el recibo del préstamo)
        [HttpGet("{id}")]
        public async Task<ActionResult<Prestamo>> GetPrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Ejemplar)
                .ThenInclude(e => e.Libro) // Incluir datos del libro
                .Include(p => p.Socio)     // Incluir datos del socio
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
                return NotFound();

            return prestamo;
        }
    }
}