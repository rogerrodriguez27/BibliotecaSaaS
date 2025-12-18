using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Prestamo
    {
        [Key]
        public int PrestamoId { get; set; }

        public int InquilinoId { get; set; }

        public int EjemplarId { get; set; }

        public int SocioId { get; set; }

        // === ¡AGREGA ESTA LÍNEA! ===
        public int RegistradoPorUsuarioId { get; set; }
        // ===========================
        [ForeignKey("RegistradoPorUsuarioId")] // <--- Pon aquí el nombre exacto del int de arriba
        public virtual Usuario? Usuario { get; set; }

        public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

        public DateTime FechaVencimiento { get; set; }

        public DateTime? FechaDevolucion { get; set; }

        public string Estado { get; set; } = "Activo";

        public Ejemplar? Ejemplar { get; set; }
        public Socio? Socio { get; set; }
    }
}
