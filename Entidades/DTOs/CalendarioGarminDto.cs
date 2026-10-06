namespace Entidades.DTOs
{
    /// <summary>Una semana de actividades de los amigos de Garmin.</summary>
    public class CalendarioGarminDto
    {
        public DateOnly Desde { get; set; }

        public DateOnly Hasta { get; set; }

        public List<DiaCalendarioGarminDto> Dias { get; set; } = [];
    }

    /// <summary>Un dia de la semana, con cada amigo en verde o en amarillo.</summary>
    public class DiaCalendarioGarminDto
    {
        public DateOnly Fecha { get; set; }

        public List<AmigoCalendarioGarminDto> Amigos { get; set; } = [];
    }

    /// <summary>Un amigo y si ese dia cargo una actividad.</summary>
    public class AmigoCalendarioGarminDto
    {
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Usuario de Garmin. Sirve para ocultar amigos en el calendario.</summary>
        public string Clave { get; set; } = string.Empty;

        public bool Cargo { get; set; }
    }
}
