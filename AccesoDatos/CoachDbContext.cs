using Entidades;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos
{
    /// <summary>
    /// Contexto de Entity Framework Core para la base de datos de CoachApp (PostgreSQL).
    /// </summary>
    public class CoachDbContext : DbContext, IDataProtectionKeyContext
    {
        public CoachDbContext(DbContextOptions<CoachDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Atleta> Atletas => Set<Atleta>();
        public DbSet<Entrenamiento> Entrenamientos => Set<Entrenamiento>();
        public DbSet<EntrenamientoAtleta> EntrenamientoAtletas => Set<EntrenamientoAtleta>();
        public DbSet<Equipo> Equipos => Set<Equipo>();
        public DbSet<EquipoMiembro> EquipoMiembros => Set<EquipoMiembro>();
        public DbSet<EntrenamientoEquipo> EntrenamientoEquipos => Set<EntrenamientoEquipo>();
        public DbSet<EntrenamientoEquipoMiembro> EntrenamientoEquipoMiembros => Set<EntrenamientoEquipoMiembro>();
        public DbSet<Anotacion> Anotaciones => Set<Anotacion>();
        public DbSet<AnotacionAtleta> AnotacionesAtleta => Set<AnotacionAtleta>();
        public DbSet<Sensacion> Sensaciones => Set<Sensacion>();
        public DbSet<Trabajo> Trabajos => Set<Trabajo>();
        public DbSet<TrabajoParcial> TrabajoParciales => Set<TrabajoParcial>();
        public DbSet<TrabajoEjercicio> TrabajoEjercicios => Set<TrabajoEjercicio>();
        public DbSet<TrabajoSerie> TrabajoSeries => Set<TrabajoSerie>();
        public DbSet<TrabajoPalada> TrabajoPaladas => Set<TrabajoPalada>();
        public DbSet<Control> Controles => Set<Control>();
        public DbSet<ControlEjercicio> ControlEjercicios => Set<ControlEjercicio>();
        public DbSet<ControlSerie> ControlSeries => Set<ControlSerie>();
        public DbSet<CuentaGarmin> CuentasGarmin => Set<CuentaGarmin>();

        /// <summary>
        /// Anillo de claves de Data Protection. En Render el disco es
        /// efimero: si las claves no viven en la base, los tokens de Garmin
        /// dejan de poder leerse en cada despliegue.
        /// </summary>
        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.NombreUsuario).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.AtletaId).IsUnique();
                entity.Property(u => u.Rol).HasConversion<string>().HasMaxLength(30);

                entity.HasOne(u => u.Atleta)
                    .WithMany()
                    .HasForeignKey(u => u.AtletaId)
                    .OnDelete(DeleteBehavior.Restrict);
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

                // La hora de Garmin es local al reloj, no UTC.
                entity.Property(ea => ea.GarminInicio).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<CuentaGarmin>(entity =>
            {
                entity.ToTable("CuentasGarmin");
                entity.HasKey(c => c.UsuarioId);
                entity.Property(c => c.AccessToken).HasColumnType("text");
                entity.Property(c => c.RefreshToken).HasColumnType("text");

                entity.HasOne(c => c.Usuario)
                    .WithOne()
                    .HasForeignKey<CuentaGarmin>(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Equipo>(entity =>
            {
                entity.ToTable("Equipos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(10);
            });

            modelBuilder.Entity<EquipoMiembro>(entity =>
            {
                entity.ToTable("EquipoMiembros");
                entity.HasKey(m => new { m.EquipoId, m.AtletaId });
                entity.HasIndex(m => new { m.EquipoId, m.Orden }).IsUnique();

                entity.HasOne(m => m.Equipo)
                    .WithMany(e => e.Miembros)
                    .HasForeignKey(m => m.EquipoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Atleta)
                    .WithMany()
                    .HasForeignKey(m => m.AtletaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EntrenamientoEquipo>(entity =>
            {
                entity.ToTable("EntrenamientoEquipos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(10);
                entity.HasIndex(e => e.EntrenamientoId);

                entity.HasOne(e => e.Entrenamiento)
                    .WithMany(en => en.Equipos)
                    .HasForeignKey(e => e.EntrenamientoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Equipo)
                    .WithMany()
                    .HasForeignKey(e => e.EquipoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<EntrenamientoEquipoMiembro>(entity =>
            {
                entity.ToTable("EntrenamientoEquipoMiembros");
                entity.HasKey(m => new { m.EntrenamientoEquipoId, m.AtletaId });
                entity.HasIndex(m => new { m.EntrenamientoEquipoId, m.Orden }).IsUnique();

                entity.HasOne(m => m.EntrenamientoEquipo)
                    .WithMany(e => e.Miembros)
                    .HasForeignKey(m => m.EntrenamientoEquipoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Atleta)
                    .WithMany()
                    .HasForeignKey(m => m.AtletaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Trabajo>(entity =>
            {
                entity.ToTable("Trabajos");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(t => new { t.EntrenamientoId, t.AtletaId });
                entity.HasIndex(t => t.EntrenamientoEquipoId);

                // Trabajo individual: cuelga del vinculo atleta-sesion.
                // Si AtletaId es null (bote), la FK compuesta no se valida.
                entity.HasOne(t => t.EntrenamientoAtleta)
                    .WithMany(ea => ea.Trabajos)
                    .HasForeignKey(t => new { t.EntrenamientoId, t.AtletaId })
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.EntrenamientoEquipo)
                    .WithMany(ee => ee.Trabajos)
                    .HasForeignKey(t => t.EntrenamientoEquipoId)
                    .IsRequired(false)
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

            modelBuilder.Entity<Control>(entity =>
            {
                entity.ToTable("Controles");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(c => new { c.AtletaId, c.Tipo, c.Fecha });

                entity.HasOne(c => c.Atleta)
                    .WithMany()
                    .HasForeignKey(c => c.AtletaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ControlEjercicio>(entity =>
            {
                entity.ToTable("ControlEjercicios");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ControlId, e.Orden });

                entity.HasOne(e => e.Control)
                    .WithMany(c => c.Ejercicios)
                    .HasForeignKey(e => e.ControlId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ControlSerie>(entity =>
            {
                entity.ToTable("ControlSeries");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.PesoKg).HasPrecision(6, 2);
                entity.HasIndex(s => new { s.ControlEjercicioId, s.Orden });

                entity.HasOne(s => s.ControlEjercicio)
                    .WithMany(e => e.Series)
                    .HasForeignKey(s => s.ControlEjercicioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AnotacionAtleta>(entity =>
            {
                entity.ToTable("AnotacionesAtleta");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Texto).HasMaxLength(4000);
                entity.HasIndex(a => a.FechaCreacion);

                entity.HasOne(a => a.Atleta)
                    .WithMany()
                    .HasForeignKey(a => a.AtletaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Sensacion>(entity =>
            {
                entity.ToTable("Sensaciones");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Texto).HasMaxLength(1000);
                entity.HasIndex(s => s.FechaCreacion);

                entity.HasOne(s => s.Atleta)
                    .WithMany()
                    .HasForeignKey(s => s.AtletaId)
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
