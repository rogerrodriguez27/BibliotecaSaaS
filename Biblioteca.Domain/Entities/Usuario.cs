using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        public int InquilinoId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty; // Hash en producción

        public string NombreCompleto { get; set; } = string.Empty;

        public string Rol { get; set; } = "Bibliotecario"; // Admin o Bibliotecario

        public bool EstaActivo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
