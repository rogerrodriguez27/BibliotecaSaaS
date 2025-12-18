using Biblioteca.API.Dtos;
using Biblioteca.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Biblioteca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration; // Necesario para leer el appsettings.json
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto login)
        {
            // 1. BUSCAR LA BIBLIOTECA (TENANT) POR SU CÓDIGO
            var tenant = await _context.Inquilinos
                .FirstOrDefaultAsync(t => t.Codigo == login.CodigoTenant);

            if (tenant == null)
                return Unauthorized("El código de la biblioteca no existe.");

            // 2. BUSCAR EL USUARIO DENTRO DE ESA BIBLIOTECA
            var usuario = await _context.Users // Usamos la tabla interna 'Users' (no Socios)
                .FirstOrDefaultAsync(u => u.Email == login.Email
                                     && u.InquilinoId == tenant.InquilinoId);

            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            // 3. VERIFICAR CONTRASEÑA
            // Nota: En producción usaríamos BCrypt.Verify(login.Password, usuario.Contrasena)
            // Para este prototipo, comparamos directo con lo que hay en BD
            if (usuario.Contrasena != login.Password)
                return Unauthorized("Usuario o contraseña incorrectos.");

            // 4. GENERAR EL TOKEN (La "Identificación Digital")
            var tokenString = GenerarTokenJWT(usuario, tenant);

            return Ok(new
            {
                token = tokenString,
                usuario = usuario.NombreCompleto,
                rol = usuario.Rol,
                inquilinoId = tenant.InquilinoId
            });
        }

        private string GenerarTokenJWT(Biblioteca.Domain.Entities.Usuario usuario, Biblioteca.Domain.Entities.Inquilino tenant)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UsuarioId", usuario.UsuarioId.ToString()),
                new Claim("InquilinoId", tenant.InquilinoId.ToString()), // ¡Vital para el SaaS!
                new Claim("Rol", usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4), // Dura 4 horas
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}