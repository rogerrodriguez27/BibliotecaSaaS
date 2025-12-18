using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER TODOS LOS LIBROS (GET: api/books)
        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
        {
            // 1. Identificar quién está preguntando
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 2. Traer SOLO sus libros
            return await _context.Libros
                .Where(l => l.InquilinoId == inquilinoId) // <--- FILTRO DE SEGURIDAD
                .ToListAsync();
        }

        // 2. CREAR UN LIBRO NUEVO (POST: api/books)
        [HttpPost]
        public async Task<ActionResult<Libro>> CreateLibro(Libro libro)
        {
            // Validamos que venga la información mínima
            if (libro.InquilinoId <= 0)
            {
                return BadRequest("Debes especificar el InquilinoId (ID de la biblioteca)");
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            // Devuelve código 201 (Created) y el libro creado
            return CreatedAtAction(nameof(GetLibros), new { id = libro.LibroId }, libro);
        }

        // PUT: api/books/5 (EDITAR)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLibro(int id, Libro libro)
        {
            // Validar seguridad
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            if (id != libro.LibroId) return BadRequest("El ID no coincide.");

            // Verificar que el libro exista y sea de mi biblioteca
            var libroExistente = await _context.Libros
                .FirstOrDefaultAsync(l => l.LibroId == id && l.InquilinoId == inquilinoId);

            if (libroExistente == null) return NotFound();

            // Actualizar campos
            libroExistente.Titulo = libro.Titulo;
            libroExistente.Autor = libro.Autor;
            libroExistente.Editorial = libro.Editorial;
            libroExistente.ISBN = libro.ISBN;
            libroExistente.AnioPublicacion = libro.AnioPublicacion;
            libroExistente.CategoriaId = libro.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent(); // 204 significa "Hecho, todo bien"
        }

        // DELETE: api/books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 1. TRAER EL LIBRO CON SUS COPIAS
            var libro = await _context.Libros
                .Include(l => l.Ejemplares)
                .FirstOrDefaultAsync(l => l.LibroId == id && l.InquilinoId == inquilinoId);

            if (libro == null) return NotFound();

            // 2. SEGURIDAD: ¿Hay libros en la calle?
            // Si alguien tiene el libro en su casa (Activo), NO dejamos borrar.
            var tienePrestamosActivos = await _context.Prestamos
                .Include(p => p.Ejemplar)
                .AnyAsync(p => p.Ejemplar.LibroId == id && p.Estado == "Activo");

            if (tienePrestamosActivos)
            {
                return BadRequest("No se puede eliminar: Hay copias prestadas actualmente a socios.");
            }

            // 3. LIMPIEZA PROFUNDA (Historial)
            // Si nadie lo tiene, borramos su rastro histórico para que SQL no se queje.

            // a) Identificar las copias de este libro
            var ejemplaresIds = libro.Ejemplares.Select(e => e.EjemplarId).ToList();

            // b) Buscar todo el historial de préstamos (incluso los devueltos)
            var historialPrestamos = await _context.Prestamos
                .Where(p => ejemplaresIds.Contains(p.EjemplarId))
                .ToListAsync();

            // c) Buscar multas asociadas a ese historial
            var multasHistorial = await _context.Multas
                .Where(m => historialPrestamos.Select(p => p.PrestamoId).Contains(m.PrestamoId))
                .ToListAsync();

            // 4. BORRAR EN ORDEN (De abajo hacia arriba)
            try
            {
                _context.Multas.RemoveRange(multasHistorial);       // 1. Adiós multas
                _context.Prestamos.RemoveRange(historialPrestamos); // 2. Adiós historial
                _context.Ejemplares.RemoveRange(libro.Ejemplares);  // 3. Adiós copias físicas
                _context.Libros.Remove(libro);                      // 4. Adiós libro padre

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error crítico al eliminar: {ex.Message}");
            }
        }
    }
}