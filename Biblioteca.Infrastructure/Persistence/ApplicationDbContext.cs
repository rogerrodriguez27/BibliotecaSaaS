using Biblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        // El constructor recibe las opciones de configuración (como la cadena de conexión)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aquí registramos las tablas. 
        // El nombre "Inquilinos" debe coincidir con tu tabla en SQL Server.
        public DbSet<Inquilino> Inquilinos { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Ejemplar> Ejemplares { get; set; }
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }

        public DbSet<Multa> Multas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aquí configuramos reglas especiales si es necesario.
            // Por ejemplo, aseguramos que la Primary Key sea InquilinoId
            modelBuilder.Entity<Inquilino>().HasKey(i => i.InquilinoId);

            // Mapeo exacto a la tabla SQL por si acaso el pluralizado falla
            modelBuilder.Entity<Inquilino>().ToTable("Inquilinos");
            modelBuilder.Entity<Libro>().ToTable("Libros");
            modelBuilder.Entity<Ejemplar>().ToTable("Ejemplares");
            modelBuilder.Entity<Socio>().ToTable("Socios");
            modelBuilder.Entity<Prestamo>().ToTable("Prestamos");
            modelBuilder.Entity<Multa>().ToTable("Multas");
        }
    }
}
