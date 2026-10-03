using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos
{
    /// <summary>
    /// Contexto de Entity Framework Core para la base de datos de CoachApp (PostgreSQL).
    /// </summary>
    public class CoachDbContext : DbContext
    {
        public CoachDbContext(DbContextOptions<CoachDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Atleta> Atletas => Set<Atleta>();
        public DbSet<Entrenamiento> Entrenamientos => Set<Entrenamiento>();
        public DbSet<EntrenamientoAtleta> EntrenamientoAtletas => Set<EntrenamientoAtleta>();
        public DbSet<Anotacion> Anotaciones => Set<Anotacion>();
        public DbSet<Trabajo> Trabajos => Set<Trabajo>();
        public DbSet<TrabajoParcial> TrabajoParciales => Set<TrabajoParcial>();
        public DbSet<TrabajoEjercicio> TrabajoEjercicios => Set<TrabajoEjercicio>();
        public DbSet<TrabajoSerie> TrabajoSeries => Set<TrabajoSerie>();
        public DbSet<TrabajoPalada> TrabajoPaladas => Set<TrabajoPalada>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.NombreUsuario).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Rol).HasConversion<string>().HasMaxLength(30);
            });

            modelBuilder.Entity<Atleta>(entity =>
            {
                entity.ToTable("Atletas");
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.Dni).IsUnique();
            });

            modelBuilder.Entity<Entrenamiento>(entity =>
            {
                entity.ToTable("Entrenamientos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Turno).HasConversion<string>().HasMaxLength(20);

                // Un dia, un turno y un numero de sesion identifican una sola sesion.
                entity.HasIndex(e => new { e.Fecha, e.Turno, e.Sesion }).IsUnique();
            });

            modelBuilder.Entity<EntrenamientoAtleta>(entity =>
            {
                entity.ToTable("EntrenamientoAtletas");
                entity.HasKey(ea => new { ea.EntrenamientoId, ea.AtletaId });

                entity.HasOne(ea => ea.Entrenamiento)
                    .WithMany(e => e.Atletas)
                    .HasForeignKey(ea => ea.EntrenamientoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ea => ea.Atleta)
                    .WithMany(a => a.Entrenamientos)
                    .HasForeignKey(ea => ea.AtletaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Trabajo>(entity =>
            {
                entity.ToTable("Trabajos");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(t => new { t.EntrenamientoId, t.AtletaId });

                // Se apoya en la clave compuesta del vinculo: un trabajo solo
                // existe si el atleta esta asignado a esa sesion.
                entity.HasOne(t => t.EntrenamientoAtleta)
                    .WithMany(ea => ea.Trabajos)
                    .HasForeignKey(t => new { t.EntrenamientoId, t.AtletaId })
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrabajoParcial>(entity =>
            {
                entity.ToTable("TrabajoParciales");
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.TrabajoId, p.Orden });

                entity.HasOne(p => p.Trabajo)
                    .WithMany(t => t.Parciales)
                    .HasForeignKey(p => p.TrabajoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrabajoEjercicio>(entity =>
            {
                entity.ToTable("TrabajoEjercicios");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TrabajoId, e.Orden });

                entity.HasOne(e => e.Trabajo)
                    .WithMany(t => t.Ejercicios)
                    .HasForeignKey(e => e.TrabajoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrabajoSerie>(entity =>
            {
                entity.ToTable("TrabajoSeries");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.PesoKg).HasPrecision(6, 2);
                entity.HasIndex(s => new { s.TrabajoEjercicioId, s.Orden });

                entity.HasOne(s => s.TrabajoEjercicio)
                    .WithMany(e => e.Series)
                    .HasForeignKey(s => s.TrabajoEjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrabajoPalada>(entity =>
            {
                entity.ToTable("TrabajoPaladas");
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.TrabajoId, p.Orden });

                entity.HasOne(p => p.Trabajo)
                    .WithMany(t => t.Paladas)
                    .HasForeignKey(p => p.TrabajoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Anotacion>(entity =>
            {
                entity.ToTable("Anotaciones");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Origen).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(a => a.FechaCreacion);
                entity.HasIndex(a => a.Origen);

                entity.HasOne(a => a.Atleta)
                    .WithMany(at => at.Anotaciones)
                    .HasForeignKey(a => a.AtletaId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Entrenamiento)
                    .WithMany(e => e.Anotaciones)
                    .HasForeignKey(a => a.EntrenamientoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Borrar un trabajo no debe borrar lo que se escribio sobre el:
                // la nota queda huerfana de trabajo pero se conserva.
                entity.HasOne(a => a.Trabajo)
                    .WithMany(t => t.Anotaciones)
                    .HasForeignKey(a => a.TrabajoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
