using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class ControlRepositorio : IControlRepositorio
    {
        private readonly CoachDbContext _context;

        public ControlRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<Control>> ObtenerAsync(int? atletaId, TipoTrabajo? tipo)
        {
            var consulta = ConDetalle(_context.Controles.AsQueryable());

            if (atletaId is not null)
            {
                consulta = consulta.Where(c => c.AtletaId == atletaId);
            }

            if (tipo is not null)
            {
                consulta = consulta.Where(c => c.Tipo == tipo);
            }

            return consulta
                .OrderByDescending(c => c.Fecha)
                .ThenByDescending(c => c.Id)
                .ToListAsync();
        }

        public Task<Control?> ObtenerPorIdAsync(int id) =>
            ConDetalle(_context.Controles.Where(c => c.Id == id))
                .FirstOrDefaultAsync();

        public Task<bool> ExisteAtletaAsync(int atletaId) =>
            _context.Atletas.AnyAsync(a => a.Id == atletaId);

        public async Task<Control> CrearAsync(Control control, IEnumerable<ControlEjercicio> ejercicios)
        {
            _context.Controles.Add(control);
            AgregarEjercicios(control, ejercicios);
            await _context.SaveChangesAsync();
            return (await ObtenerPorIdAsync(control.Id))!;
        }

        public async Task ActualizarAsync(Control control, IEnumerable<ControlEjercicio> ejercicios)
        {
            _context.ControlSeries.RemoveRange(
                control.Ejercicios.SelectMany(e => e.Series));
            _context.ControlEjercicios.RemoveRange(control.Ejercicios);
            control.Ejercicios.Clear();

            AgregarEjercicios(control, ejercicios);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Control control)
        {
            _context.Controles.Remove(control);
            await _context.SaveChangesAsync();
        }

        private static IQueryable<Control> ConDetalle(IQueryable<Control> consulta) =>
            consulta
                .Include(c => c.Atleta)
                .Include(c => c.Ejercicios)
                    .ThenInclude(e => e.Series);

        private static void AgregarEjercicios(Control control, IEnumerable<ControlEjercicio> ejercicios)
        {
            var orden = 1;
            foreach (var ejercicio in ejercicios)
            {
                var nuevo = new ControlEjercicio
                {
                    Nombre = ejercicio.Nombre.Trim(),
                    Orden = orden++
                };

                var ordenSerie = 1;
                foreach (var serie in ejercicio.Series)
                {
                    nuevo.Series.Add(new ControlSerie
                    {
                        Repeticiones = serie.Repeticiones,
                        PesoKg = serie.PesoKg,
                        EsUnoRm = serie.EsUnoRm,
                        Orden = ordenSerie++
                    });
                }

                control.Ejercicios.Add(nuevo);
            }
        }
    }
}
