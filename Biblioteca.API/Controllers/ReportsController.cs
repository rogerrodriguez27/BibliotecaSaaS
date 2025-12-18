using Biblioteca.API.Dtos;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/reports/dashboard?inquilinoId=1
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 1. Contadores Generales
            var totalLibros = await _context.Libros.CountAsync(x => x.InquilinoId == inquilinoId);
            var totalSocios = await _context.Socios.CountAsync(x => x.InquilinoId == inquilinoId);

            var prestamosActivos = await _context.Prestamos
                .CountAsync(x => x.InquilinoId == inquilinoId && x.Estado == "Activo");

            var prestamosVencidos = await _context.Prestamos
                .CountAsync(x => x.InquilinoId == inquilinoId &&
                                 x.Estado == "Activo" &&
                                 x.FechaVencimiento < DateTime.Now);

            // 2. NUEVO: Traer los últimos 5 movimientos recientes
            var ultimosPrestamos = await _context.Prestamos
                .Include(p => p.Socio)
                .Include(p => p.Ejemplar).ThenInclude(e => e.Libro)
                .Where(x => x.InquilinoId == inquilinoId)
                .OrderByDescending(x => x.FechaPrestamo) // Los más nuevos primero
                .Take(5) // Solo los 5 últimos
                .Select(p => new
                {
                    p.PrestamoId,
                    Libro = p.Ejemplar.Libro.Titulo,
                    Socio = p.Socio.NombreCompleto,
                    Fecha = p.FechaPrestamo.ToString("dd/MM/yyyy"), // Formato bonito
                    Estado = p.Estado
                })
                .ToListAsync();

            return Ok(new
            {
                totalLibros,
                totalSocios,
                prestamosActivos,
                prestamosVencidos,
                ultimosPrestamos // <--- Enviamos la lista al frontend
            });
        }

        // GET: api/reports/socio/5
        [HttpGet("socio/{socioId}")]
        public async Task<ActionResult<SocioEstadisticasDto>> GetSocioStats(int socioId)
        {
            // 1. Validar identidad (Inquilino)
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 2. Buscar al socio
            var socio = await _context.Socios
                .FirstOrDefaultAsync(s => s.SocioId == socioId && s.InquilinoId == inquilinoId);

            if (socio == null) return NotFound("Socio no encontrado.");

            // 3. Calcular estadísticas
            var stats = new SocioEstadisticasDto
            {
                SocioId = socio.SocioId,
                NombreCompleto = socio.NombreCompleto,

                // Cuántos tiene sin devolver
                PrestamosActivos = await _context.Prestamos
                    .CountAsync(p => p.SocioId == socioId && p.Estado == "Activo"),

                // Cuántos ha pedido en toda su historia
                HistorialPrestamos = await _context.Prestamos
                    .CountAsync(p => p.SocioId == socioId),

                // Cuántas veces devolvió tarde (FechaDevolucion > FechaVencimiento)
                DevolucionesTardias = await _context.Prestamos
                    .CountAsync(p => p.SocioId == socioId && p.FechaDevolucion > p.FechaVencimiento),

                // Dinero que debe en multas
                MultasPendientes = await _context.Multas
                    .Include(m => m.Prestamo)
                    .Where(m => m.Prestamo.SocioId == socioId && m.Estado == "Pendiente")
                    .SumAsync(m => m.Monto)
            };

            return Ok(stats);
        }
    }
}