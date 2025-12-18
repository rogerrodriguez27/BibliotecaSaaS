using Biblioteca.Domain.Entities;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Solo usuarios logueados pueden ver/crear otros usuarios
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users (Listar compañeros de trabajo)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Filtramos para ver solo usuarios de MI biblioteca (SaaS)
            // Leemos el InquilinoId del token
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            int inquilinoId = int.Parse(inquilinoIdClaim.Value);

            return await _context.Users
                .Where(u => u.InquilinoId == inquilinoId)
                .ToListAsync();
        }

        // POST: api/users (Registrar nuevo bibliotecario)
        [HttpPost]
        public async Task<ActionResult<Usuario>> CreateUsuario(Usuario usuario)
        {
            // 1. Asignar automáticamente el Inquilino del admin que lo crea
            var inquilinoIdClaim = User.FindFirst("InquilinoId");
            if (inquilinoIdClaim == null) return Unauthorized();
            usuario.InquilinoId = int.Parse(inquilinoIdClaim.Value);

            // 2. Validar que el email no exista ya
            bool existe = await _context.Users
                .AnyAsync(u => u.Email == usuario.Email && u.InquilinoId == usuario.InquilinoId);

            if (existe) return BadRequest("Ese correo ya está registrado en esta biblioteca.");

            // 3. Defaults
            usuario.FechaCreacion = DateTime.UtcNow;
            usuario.EstaActivo = true;
            if (string.IsNullOrEmpty(usuario.Rol)) usuario.Rol = "Bibliotecario";

            _context.Users.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuarios), new { id = usuario.UsuarioId }, usuario);
        }
    }
}