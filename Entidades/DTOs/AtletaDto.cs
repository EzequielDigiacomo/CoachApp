namespace Entidades.DTOs
{
    /// <summary>Datos publicos de un atleta.</summary>
    public class AtletaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }

        /// <summary>Edad en anios, calculada al momento de la consulta.</summary>
        public int Edad { get; set; }

        /// <summary>Nombres de las categorias que cubre esa edad.</summary>
        public List<string> Categorias { get; set; } = [];

        public string? Club { get; set; }
        public string? Email { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
