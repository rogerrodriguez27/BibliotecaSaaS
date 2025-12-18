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
    }
}