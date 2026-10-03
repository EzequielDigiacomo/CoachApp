using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Datos para dar de alta un atleta.</summary>
    public class CrearAtletaRequest : IValidatableObject
    {
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public DateOnly FechaNacimiento { get; set; }

        [MaxLength(150)]
        public string? Club
        {
            get => _club;
            set => _club = Normalizar(value);
        }

        [MaxLength(150), EmailAddress]
        public string? Email
        {
            get => _email;
            set => _email = Normalizar(value);
        }

        [Required, MaxLength(20)]
        public string Dni { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Telefono
        {
            get => _telefono;
            set => _telefono = Normalizar(value);
        }

        private string? _club;
        private string? _email;
        private string? _telefono;

        /// <summary>
        /// Un campo opcional que llega vacio se guarda como null. Sin esto, el
        /// frontend mandando "" hace fallar la validacion de e-mail.
        /// </summary>
        private static string? Normalizar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            if (FechaNacimiento > hoy)
            {
                yield return new ValidationResult(
                    "La fecha de nacimiento no puede ser futura.",
                    new[] { nameof(FechaNacimiento) });
            }

            if (FechaNacimiento < hoy.AddYears(-120))
            {
                yield return new ValidationResult(
                    "La fecha de nacimiento no parece valida.",
                    new[] { nameof(FechaNacimiento) });
            }
        }
    }
}
