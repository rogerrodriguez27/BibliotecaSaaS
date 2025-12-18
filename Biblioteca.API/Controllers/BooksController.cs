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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
        {
            // OJO: En el futuro aquí filtraremos por el usuario logueado.
            // Por ahora, devuelve todo lo que haya en la base de datos.
            return await _context.Libros.ToListAsync();
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

        // DELETE: api/books/5 (ELIMINAR)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLibro(int id)
        {
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            var libro = await _context.Libros
                .FirstOrDefaultAsync(l => l.LibroId == id && l.InquilinoId == inquilinoId);

            if (libro == null) return NotFound();

            try
            {
                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                // Si falla (ej: tiene copias o préstamos), avisamos
                return BadRequest("No se puede eliminar el libro porque tiene copias o registros asociados.");
            }
        }
    }
}