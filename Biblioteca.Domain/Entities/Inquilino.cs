using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Inquilino
    {
        // Coincide con - InquilinoId
        public int InquilinoId { get; set; }

        // Coincide con - Nombre
        public string Nombre { get; set; } = string.Empty;

        // Coincide con - Codigo (ej: USC2024)
        public string Codigo { get; set; } = string.Empty;

        // Coincide con - Email de contacto
        public string? Email { get; set; }

        public string? Telefono { get; set; }

        public bool EstaActivo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
