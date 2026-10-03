using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Lista de atletas que se suman a una sesion.</summary>
    public class AsignarAtletasRequest
    {
        [Required]
        public List<int> AtletaIds { get; set; } = [];
    }
}
