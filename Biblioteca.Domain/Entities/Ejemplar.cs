using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Ejemplar
    {
        [Key]
        public int EjemplarId { get; set; }

        public int InquilinoId { get; set; }

        public int LibroId { get; set; }

        public string CodigoBarras { get; set; } = string.Empty;

        public string Estado { get; set; } = "Disponible"; // Disponible, Prestado, Mantenimiento

        public string? Ubicacion { get; set; }

        // Navegación (para poder acceder a los datos del Libro desde la copia)
        public Libro? Libro { get; set; }
    }
}
