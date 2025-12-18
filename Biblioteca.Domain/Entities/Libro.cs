using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Libro
    {
        [Key]
        public int LibroId { get; set; }

        public int InquilinoId { get; set; } // ¡Vital para el SaaS!

        public string Titulo { get; set; } = string.Empty;

        public string? Autor { get; set; }

        public string? Editorial { get; set; }

        public string? ISBN { get; set; }

        public int? AnioPublicacion { get; set; }

        public int? CategoriaId { get; set; }

        // Relación: Un libro tiene muchas copias físicas (Ejemplares)
        public ICollection<Ejemplar> Ejemplares { get; set; } = new List<Ejemplar>();
    }
}
