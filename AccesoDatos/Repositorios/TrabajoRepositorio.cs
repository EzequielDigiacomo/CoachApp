using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class TrabajoRepositorio : ITrabajoRepositorio
    {
        private readonly CoachDbContext _context;

        public TrabajoRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<Trabajo>> ObtenerPorAtletaAsync(int entrenamientoId, int atletaId) =>
            ConDetalle(_context.Trabajos
                .Where(t => t.EntrenamientoId == entrenamientoId && t.AtletaId == atletaId))
                .OrderBy(t => t.HoraInicio)
                .ThenBy(t => t.Id)
                .ToListAsync();

        public Task<Trabajo?> ObtenerPorIdAsync(int trabajoId) =>
            ConDetalle(_context.Trabajos.Where(t => t.Id == trabajoId))
                .FirstOrDefaultAsync();

        public Task<List<Trabajo>> ObtenerHistorialAsync(int atletaId) =>
            ConDetalle(_context.Trabajos.Where(t => t.AtletaId == atletaId))
                // Las sesiones, de la mas nueva a la mas vieja.
                .OrderByDescending(t => t.EntrenamientoAtleta.Entrenamiento.Fecha)
                .ThenByDescending(t => t.EntrenamientoAtleta.Entrenamiento.Turno)
                .ThenByDescending(t => t.EntrenamientoAtleta.Entrenamiento.Sesion)
                // Dentro de la sesion, los trabajos en el orden en que se hicieron.
                .ThenBy(t => t.HoraInicio)
                .ThenBy(t => t.Id)
                .ToListAsync();

        public Task<bool> ExisteVinculoAsync(int entrenamientoId, int atletaId) =>
            _context.EntrenamientoAtletas
                .AnyAsync(ea => ea.EntrenamientoId == entrenamientoId && ea.AtletaId == atletaId);

        public async Task<Trabajo> CrearAsync(
            Trabajo trabajo,
            IEnumerable<TrabajoParcial> parciales,
            IEnumerable<TrabajoEjercicio> ejercicios,
            IEnumerable<TrabajoPalada> paladas)
        {
            _context.Trabajos.Add(trabajo);

            var orden = 1;
            foreach (var parcial in parciales)
            {
                trabajo.Parciales.Add(new TrabajoParcial
                {
                    DistanciaMetros = parcial.DistanciaMetros,
                    Tiempo = parcial.Tiempo,
                    Orden = orden++
                });
            }

            orden = 1;
            foreach (var ejercicio in ejercicios)
            {
                trabajo.Ejercicios.Add(ArmarEjercicio(ejercicio, orden++));
            }

            orden = 1;
            foreach (var palada in paladas)
            {
                trabajo.Paladas.Add(new TrabajoPalada
                {
                    Tiempo = palada.Tiempo,
                    Ppm = palada.Ppm,
                    Orden = orden++
                });
            }

            await _context.SaveChangesAsync();
            return trabajo;
        }

        public async Task ActualizarAsync(
            Trabajo trabajo,
            IEnumerable<TrabajoParcial> parciales,
            IEnumerable<TrabajoEjercicio> ejercicios,
            IEnumerable<TrabajoPalada> paladas)
        {
            // Se reemplazan enteros: el modal guarda la grilla completa, asi que
            // no vale la pena diferenciar fila por fila. Las series se van con su
            // ejercicio por la cascada.
            var parcialesViejos = await _context.TrabajoParciales
                .Where(p => p.TrabajoId == trabajo.Id)
                .ToListAsync();

            var ejerciciosViejos = await _context.TrabajoEjercicios
                .Where(e => e.TrabajoId == trabajo.Id)
                .ToListAsync();

            var paladasViejas = await _context.TrabajoPaladas
                .Where(p => p.TrabajoId == trabajo.Id)
                .ToListAsync();

            _context.TrabajoParciales.RemoveRange(parcialesViejos);
            _context.TrabajoEjercicios.RemoveRange(ejerciciosViejos);
            _context.TrabajoPaladas.RemoveRange(paladasViejas);

            var orden = 1;
            foreach (var parcial in parciales)
            {
                _context.TrabajoParciales.Add(new TrabajoParcial
                {
                    TrabajoId = trabajo.Id,
                    DistanciaMetros = parcial.DistanciaMetros,
                    Tiempo = parcial.Tiempo,
                    Orden = orden++
                });
            }

            orden = 1;
            foreach (var ejercicio in ejercicios)
            {
                var nuevo = ArmarEjercicio(ejercicio, orden++);
                nuevo.TrabajoId = trabajo.Id;
                _context.TrabajoEjercicios.Add(nuevo);
            }

            orden = 1;
            foreach (var palada in paladas)
            {
                _context.TrabajoPaladas.Add(new TrabajoPalada
                {
                    TrabajoId = trabajo.Id,
                    Tiempo = palada.Tiempo,
                    Ppm = palada.Ppm,
                    Orden = orden++
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Trabajo trabajo)
        {
            _context.Trabajos.Remove(trabajo);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Arma un ejercicio con sus series ya ordenadas. El Id y el TrabajoId
        /// los completa EF segun corresponda.
        /// </summary>
        private static TrabajoEjercicio ArmarEjercicio(TrabajoEjercicio ejercicio, int orden)
        {
            var nuevo = new TrabajoEjercicio
            {
                Nombre = ejercicio.Nombre,
                Orden = orden
            };

            var ordenSerie = 1;
            foreach (var serie in ejercicio.Series)
            {
                nuevo.Series.Add(new TrabajoSerie
                {
                    Repeticiones = serie.Repeticiones,
                    Porcentaje = serie.Porcentaje,
                    PesoKg = serie.PesoKg,
                    Orden = ordenSerie++
                });
            }

            return nuevo;
        }

        /// <summary>Trae el trabajo con parciales, ejercicios, paladas y su sesion.</summary>
        private static IQueryable<Trabajo> ConDetalle(IQueryable<Trabajo> query) =>
            query
                .Include(t => t.Parciales)
                .Include(t => t.Ejercicios)
                    .ThenInclude(e => e.Series)
                .Include(t => t.Paladas)
                .Include(t => t.EntrenamientoAtleta)
                    .ThenInclude(ea => ea.Entrenamiento);
    }
}
