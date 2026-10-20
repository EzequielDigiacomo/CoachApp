using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Entidades.DTOs
{
    /// <summary>Asignacion masiva de colores a atletas.</summary>
    public class GuardarColoresAtletasRequest : IValidatableObject
    {
        [Required, MinLength(1, ErrorMessage = "Indicá al menos un atleta.")]
        public List<ColorAtletaItem> Colores { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            for (var i = 0; i < Colores.Count; i++)
            {
                var item = Colores[i];
                if (item.AtletaId <= 0)
                {
                    yield return new ValidationResult(
                        $"El ítem {i + 1} tiene un atleta inválido.",
                        new[] { nameof(Colores) });
                }

                if (item.Color is not null && !EsColorHex(item.Color))
                {
                    yield return new ValidationResult(
                        $"El color del ítem {i + 1} debe ser #RRGGBB o vacío.",
                        new[] { nameof(Colores) });
                }
            }
        }

        public static bool EsColorHex(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return true;
            }

            return Regex.IsMatch(color.Trim(), "^#[0-9A-Fa-f]{6}$");
        }

        public static string? Normalizar(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
            {
                return null;
            }

            return color.Trim().ToUpperInvariant();
        }
    }

    public class ColorAtletaItem
    {
        public int AtletaId { get; set; }

        /// <summary>#RRGGBB o null para quitar el color.</summary>
        public string? Color { get; set; }
    }
}
