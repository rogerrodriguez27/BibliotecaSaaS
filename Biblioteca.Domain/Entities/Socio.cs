using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Socio
    {
        [Key]
        public int SocioId { get; set; }

        public int InquilinoId { get; set; }

        public string Codigo { get; set; } = string.Empty; // DNI o Matrícula

        public string NombreCompleto { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string TipoSocio { get; set; } = "Estudiante";

        public string Estado { get; set; } = "Activo";
    }
}
