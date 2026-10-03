using Entidades.DTOs;
using Entidades.Util;

namespace Entidades.Mapeos
{
    /// <summary>Conversion de la entidad Anotacion a su DTO publico.</summary>
    public static class AnotacionMapeos
    {
        /// <summary>
        /// Arma el DTO. Los vinculos se completan solo si la entidad viene con
        /// esas relaciones cargadas; si no, quedan en null.
        /// </summary>
        public static AnotacionDto ToDto(this Anotacion anotacion)
        {
            return new AnotacionDto
            {
                Id = anotacion.Id,
                Titulo = anotacion.Titulo,
                Texto = anotacion.Texto,
                Origen = anotacion.Origen,
                RequiereRevision = anotacion.RequiereRevision,
                FechaCreacion = anotacion.FechaCreacion,

                AtletaId = anotacion.AtletaId,
                AtletaNombre = anotacion.Atleta?.Nombre,
                AtletaApellido = anotacion.Atleta?.Apellido,

                EntrenamientoId = anotacion.EntrenamientoId,
                EntrenamientoFecha = anotacion.Entrenamiento?.Fecha,
                EntrenamientoTurno = anotacion.Entrenamiento?.Turno,
                EntrenamientoSesion = anotacion.Entrenamiento?.Sesion,

                TrabajoId = anotacion.TrabajoId,
                TrabajoTipo = anotacion.Trabajo?.Tipo,
                TrabajoHoraInicio = anotacion.Trabajo?.HoraInicio is null
                    ? null
                    : TiempoTexto.FormatearHora(anotacion.Trabajo.HoraInicio.Value),

                // El usuario solo se usa para mostrar quien la escribio.
                UsuarioId = anotacion.UsuarioId,
                UsuarioNombre = anotacion.Usuario?.NombreUsuario
            };
        }
    }
}
