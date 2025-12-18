using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LoansController(ApplicationDbContext context)
        {
            _context = context;
        }


        // POST: api/Loans
        [HttpPost]
        public async Task<ActionResult<Prestamo>> PostPrestamo(Prestamo prestamo)
        {
            // 1. Obtener Inquilino del Token
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 2. Obtener Usuario (Bibliotecario) del Token ¡ESTO ES LO NUEVO!
            var usuarioIdClaim = User.FindFirst("UsuarioId");
            if (usuarioIdClaim == null) return Unauthorized("No se identificó al usuario que registra.");
            int usuarioId = int.Parse(usuarioIdClaim.Value);

            // 3. Asignar los IDs de seguridad al objeto
            prestamo.InquilinoId = inquilinoId;

            // IMPORTANTE: Verifica si en tu modelo 'Prestamo.cs' la propiedad se llama 
            // "UsuarioId" o "RegistradoPorUsuarioId". 
            // Basado en tu error FK_Prestamos_Usuarios, debería ser una de las dos.
            // Usaré 'UsuarioId' como ejemplo, si te marca rojo, cámbialo a 'RegistradoPorUsuarioId'.
            prestamo.RegistradoPorUsuarioId = usuarioId; // <--- AQUÍ ESTABA EL ERROR

            prestamo.Estado = "Activo";

            // 4. VALIDAR FECHAS
            if (prestamo.FechaPrestamo == default)
            {
                prestamo.FechaPrestamo = DateTime.Now;
            }

            if (prestamo.FechaVencimiento == default)
            {
                prestamo.FechaVencimiento = prestamo.FechaPrestamo.AddDays(7);
            }

            if (prestamo.FechaVencimiento < prestamo.FechaPrestamo)
            {
                return BadRequest("La fecha de vencimiento no puede ser anterior a la fecha de préstamo.");
            }

            // 5. Validar disponibilidad del libro
            var copia = await _context.Ejemplares.FindAsync(prestamo.EjemplarId);
            if (copia == null || copia.Estado != "Disponible")
            {
                return BadRequest("El libro no está disponible para préstamo.");
            }

            // 6. Guardar
            _context.Prestamos.Add(prestamo);

            // Actualizar estado del libro
            copia.Estado = "Prestado";

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPrestamo", new { id = prestamo.PrestamoId }, prestamo);
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

        // GET: api/Loans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamos()
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            return await _context.Prestamos
                .Include(p => p.Socio)        // Datos del lector
                .Include(p => p.Usuario)      // <--- ¡NUEVO! Datos del bibliotecario responsable
                .Include(p => p.Ejemplar)
                    .ThenInclude(e => e.Libro)
                .Where(p => p.InquilinoId == inquilinoId)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync();
        }

        // POST: api/loans/return (DEVOLVER LIBRO)
        [HttpPost("return")]
        public async Task<ActionResult> ReturnLibro([FromBody] int prestamoId)
        {
            // 1. BUSCAR EL PRÉSTAMO ACTIVO
            var prestamo = await _context.Prestamos
                .Include(p => p.Ejemplar)
                .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

            if (prestamo == null)
                return NotFound("Préstamo no encontrado.");

            if (prestamo.Estado == "Devuelto")
                return BadRequest("Este préstamo ya fue devuelto anteriormente.");

            // 2. REGISTRAR FECHA DE DEVOLUCIÓN
            prestamo.FechaDevolucion = DateTime.UtcNow;
            prestamo.Estado = "Devuelto";

            // 3. LIBERAR EL EJEMPLAR (Para que otro lo pueda pedir)
            if (prestamo.Ejemplar != null)
            {
                prestamo.Ejemplar.Estado = "Disponible";
            }

            // 4. CALCULAR MULTAS (Si entregó tarde)
            if (prestamo.FechaDevolucion > prestamo.FechaVencimiento)
            {
                var diasRetraso = (prestamo.FechaDevolucion.Value - prestamo.FechaVencimiento).Days;

                if (diasRetraso > 0)
                {
                    var multa = new Multa
                    {
                        PrestamoId = prestamo.PrestamoId,
                        DiasRetraso = diasRetraso,
                        Monto = diasRetraso * 1.50m, // Ejemplo: $1.50 por día de retraso
                        Estado = "Pendiente"
                    };
                    _context.Multas.Add(multa);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Libro devuelto exitosamente", prestamoId = prestamo.PrestamoId });
        }
    }

}