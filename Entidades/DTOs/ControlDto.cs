using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>Control independiente de sesion (gim o agua).</summary>
    public class ControlDto
    {
        public int Id { get; set; }

        public int AtletaId { get; set; }

        public string? AtletaNombre { get; set; }

        public string? AtletaColor { get; set; }

        public TipoTrabajo Tipo { get; set; }

        public DateOnly Fecha { get; set; }

        public bool EsAutoCarga { get; set; }

        /// <summary>Distancia del control de agua (m).</summary>
        public int? DistanciaMetros { get; set; }

        /// <summary>Tiempo logrado, ya formateado ("2:16" / "2:16.5").</summary>
        public string? Tiempo { get; set; }

        /// <summary>Tiempo testigo de referencia, formateado.</summary>
        public string? TiempoTestigo { get; set; }

        /// <summary>Año del tiempo testigo.</summary>
        public int? AnioTestigo { get; set; }

        /// <summary>Velocidad promedio en km/h, calculada con distancia y tiempo.</summary>
        public decimal? VelocidadKmh { get; set; }

        /// <summary>Categoria etaria del atleta al momento de la consulta.</summary>
        public string? Categoria { get; set; }

        /// <summary>Diferencia con el testigo (negativo = mas rapido). Formateada.</summary>
        public string? DiferenciaTestigo { get; set; }

        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; }

        public List<ControlEjercicioDto> Ejercicios { get; set; } = [];
    }

    public class ControlEjercicioDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Orden { get; set; }

        public List<ControlSerieDto> Series { get; set; } = [];
    }

    public class ControlSerieDto
    {
        public int Id { get; set; }

        public int Repeticiones { get; set; }

        public decimal? PesoKg { get; set; }

        public bool EsUnoRm { get; set; }

        public int Orden { get; set; }
    }
}
