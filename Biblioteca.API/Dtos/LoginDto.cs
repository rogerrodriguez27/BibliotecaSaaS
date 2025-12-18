namespace Biblioteca.API.Dtos
{
    public class LoginDto
    {
        public string CodigoTenant { get; set; } = string.Empty; // Ej: USC2024
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}