using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.DTOs
{
    public class EquipoMiembroDto
    {
        public int AtletaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Orden { get; set; }
    }

    public class EquipoDto
    {
        public int Id { get; set; }
        public TipoBote Tipo { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<EquipoMiembroDto> Miembros { get; set; } = [];
    }

    public class GuardarEquipoRequest
    {
        public TipoBote Tipo { get; set; }

        [MinLength(2)]
        public List<int> AtletaIds { get; set; } = [];
    }

    public class EquipoEnEntrenamientoDto
    {
        public int Id { get; set; }
        public TipoBote Tipo { get; set; }
        public string Etiqueta { get; set; } = string.Empty;
        public int? EquipoId { get; set; }
        public List<EquipoMiembroDto> Miembros { get; set; } = [];
        public int CantidadTrabajos { get; set; }
        public List<ResumenTrabajoDto> ResumenTrabajos { get; set; } = [];
    }

    public class CargarEquipoSesionRequest
    {
        /// <summary>Bote del catalogo. Si viene, se copian sus miembros.</summary>
        public int? EquipoId { get; set; }

        /// <summary>Armar uno nuevo con estos atletas de la sesion.</summary>
        public TipoBote? Tipo { get; set; }

        public List<int> AtletaIds { get; set; } = [];
    }
}
