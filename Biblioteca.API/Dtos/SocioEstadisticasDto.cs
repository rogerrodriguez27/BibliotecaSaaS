namespace Biblioteca.API.Dtos
{
    public class SocioEstadisticasDto
    {
        public int SocioId { get; set; }
        public string NombreCompleto { get; set; }
        public int PrestamosActivos { get; set; } // Libros que tiene en casa ahora
        public int HistorialPrestamos { get; set; } // Total histórico
        public int DevolucionesTardias { get; set; }
        public decimal MultasPendientes { get; set; }
    }
}
