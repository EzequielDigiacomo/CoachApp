using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class EquipoRepositorio : IEquipoRepositorio
    {
        private readonly CoachDbContext _context;

        public EquipoRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<Equipo>> ListarAsync(bool incluirInactivos = false)
        {
            var consulta = ConMiembros(_context.Equipos);
            if (!incluirInactivos)
            {
                consulta = consulta.Where(e => e.Activo);
            }

            return consulta
                .OrderBy(e => e.Tipo)
                .ThenBy(e => e.Id)
                .ToListAsync();
        }

        public Task<Equipo?> ObtenerAsync(int id) =>
            ConMiembros(_context.Equipos).FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Equipo> CrearAsync(TipoBote tipo, IReadOnlyList<int> atletaIds)
        {
            var equipo = new Equipo
            {
                Tipo = tipo,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            for (var i = 0; i < atletaIds.Count; i++)
            {
                equipo.Miembros.Add(new EquipoMiembro
                {
                    AtletaId = atletaIds[i],
                    Orden = i + 1
                });
            }

            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();
            return (await ObtenerAsync(equipo.Id))!;
        }

        public async Task<Equipo?> ActualizarAsync(int id, TipoBote tipo, IReadOnlyList<int> atletaIds)
        {
            var equipo = await _context.Equipos
                .Include(e => e.Miembros)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (equipo is null)
            {
                return null;
            }

            equipo.Tipo = tipo;
            _context.EquipoMiembros.RemoveRange(equipo.Miembros);
            equipo.Miembros.Clear();

            for (var i = 0; i < atletaIds.Count; i++)
            {
                equipo.Miembros.Add(new EquipoMiembro
                {
                    EquipoId = equipo.Id,
                    AtletaId = atletaIds[i],
                    Orden = i + 1
                });
            }

            await _context.SaveChangesAsync();
            return await ObtenerAsync(equipo.Id);
        }

        public async Task<bool> CambiarEstadoAsync(int id, bool activo)
        {
            var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id);
            if (equipo is null)
            {
                return false;
            }

            equipo.Activo = activo;
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<Equipo> ConMiembros(IQueryable<Equipo> consulta) =>
            consulta.Include(e => e.Miembros).ThenInclude(m => m.Atleta);
    }
}
