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
        public async Task<ActionResult<DashboardResumen>> GetDashboardStats([FromQuery] int inquilinoId)
        {
            if (inquilinoId <= 0) return BadRequest("Se requiere el InquilinoId");

            var stats = new DashboardResumen();

            // 1. Total de Libros (Títulos)
            stats.TotalLibros = await _context.Libros
                .CountAsync(l => l.InquilinoId == inquilinoId);

            // 2. Total de Socios Activos
            stats.TotalSocios = await _context.Socios
                .CountAsync(s => s.InquilinoId == inquilinoId && s.Estado == "Activo");

            // 3. Préstamos Activos (Libros en la calle)
            stats.PrestamosActivos = await _context.Prestamos
                .CountAsync(p => p.InquilinoId == inquilinoId && p.Estado == "Activo");

            // 4. Préstamos Vencidos (Fecha Vencimiento < Hoy)
            // Nota: Solo contamos los que siguen "Activos" (no devueltos) pero ya vencieron
            stats.PrestamosVencidos = await _context.Prestamos
                .CountAsync(p => p.InquilinoId == inquilinoId
                                 && p.Estado == "Activo"
                                 && p.FechaVencimiento < DateTime.UtcNow);

            // 5. Total Dinero en Multas (Pendientes)
            // Nota: Aquí sumamos el dinero, no la cantidad de multas
            // Usamos Join porque la multa no tiene InquilinoId directo, llegamos a través del Préstamo
            stats.TotalMultasPendientes = await _context.Multas
                .Include(m => m.Prestamo)
                .Where(m => m.Estado == "Pendiente" && m.Prestamo.InquilinoId == inquilinoId)
                .SumAsync(m => m.Monto);

            return Ok(stats);
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