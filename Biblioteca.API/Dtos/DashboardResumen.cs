namespace Biblioteca.API.Dtos
{
    public class DashboardResumen
    {
        public int TotalLibros { get; set; }
        public int TotalSocios { get; set; }
        public int PrestamosActivos { get; set; }
        public int PrestamosVencidos { get; set; }
        public decimal TotalMultasPendientes { get; set; }
    }
}