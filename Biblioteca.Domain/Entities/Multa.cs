using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Domain.Entities
{
    public class Multa
    {
        [Key]
        public int MultaId { get; set; }

        public int PrestamoId { get; set; }

        public int DiasRetraso { get; set; }

        public decimal Monto { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relación con el préstamo
        public Prestamo? Prestamo { get; set; }
    }
}
