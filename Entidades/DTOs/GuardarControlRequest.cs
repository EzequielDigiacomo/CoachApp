using System.ComponentModel.DataAnnotations;
using Entidades.Enums;
using Entidades.Util;

namespace Entidades.DTOs
{
    /// <summary>Datos para crear o modificar un control.</summary>
    public class GuardarControlRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Elegí el atleta.")]
        public int? AtletaId { get; set; }

        [Required(ErrorMessage = "Elegí el tipo de control: gimnasio, tierra o agua.")]
        public TipoTrabajo? Tipo { get; set; }

        [Required(ErrorMessage = "Indicá la fecha del control.")]
        public DateOnly? Fecha { get; set; }

        /// <summary>Sin kilos ni 1RM: solo ejercicio y repeticiones (gimnasio).</summary>
        public bool EsAutoCarga { get; set; }

        /// <summary>Distancia en metros (agua / tierra).</summary>
        public int? DistanciaMetros { get; set; }

        /// <summary>Tiempo logrado (agua / tierra), ej. "2:16" o "2:16,5".</summary>
        public string? Tiempo { get; set; }

        /// <summary>Tiempo testigo de referencia (agua).</summary>
        public string? TiempoTestigo { get; set; }

        /// <summary>Año del tiempo testigo (agua).</summary>
        public int? AnioTestigo { get; set; }

        [MaxLength(300, ErrorMessage = "Las observaciones no pueden superar los 300 caracteres.")]
        public string? Observaciones { get; set; }

        public List<GuardarControlEjercicioRequest> Ejercicios { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tipo is TipoTrabajo.Agua or TipoTrabajo.Tierra)
            {
                if (DistanciaMetros is null or <= 0)
                {
                    yield return new ValidationResult(
                        "Indicá la distancia en metros.",
                        new[] { nameof(DistanciaMetros) });
                }

                if (string.IsNullOrWhiteSpace(Tiempo) || !TiempoTexto.IntentaParse(Tiempo, out _))
                {
                    yield return new ValidationResult(
                        "El tiempo no es válido. Escribilo como 2:16 o 2:16,5.",
                        new[] { nameof(Tiempo) });
                }

                if (Tipo == TipoTrabajo.Agua)
                {
                    if (!string.IsNullOrWhiteSpace(TiempoTestigo) && !TiempoTexto.IntentaParse(TiempoTestigo, out _))
                    {
                        yield return new ValidationResult(
                            "El tiempo testigo no es válido. Escribilo como 2:16 o 2:16,5.",
                            new[] { nameof(TiempoTestigo) });
                    }

                    if (AnioTestigo is not null && (AnioTestigo < 1990 || AnioTestigo > 2100))
                    {
                        yield return new ValidationResult(
                            "El año del testigo no es válido.",
                            new[] { nameof(AnioTestigo) });
                    }
                }

                yield break;
            }

            if (Tipo != TipoTrabajo.Gimnasio)
            {
                yield break;
            }

            if (Ejercicios.Count == 0)
            {
                yield return new ValidationResult(
                    "Agregá al menos un ejercicio.",
                    new[] { nameof(Ejercicios) });
                yield break;
            }

            for (var i = 0; i < Ejercicios.Count; i++)
            {
                var ejercicio = Ejercicios[i];
                if (string.IsNullOrWhiteSpace(ejercicio.Nombre))
                {
                    yield return new ValidationResult(
                        $"El ejercicio {i + 1} necesita un nombre.",
                        new[] { nameof(Ejercicios) });
                }

                if (ejercicio.Series.Count == 0)
                {
                    yield return new ValidationResult(
                        $"El ejercicio {i + 1} necesita al menos una serie.",
                        new[] { nameof(Ejercicios) });
                    continue;
                }

                var marcadas = 0;
                for (var j = 0; j < ejercicio.Series.Count; j++)
                {
                    var serie = ejercicio.Series[j];
                    if (serie.Repeticiones <= 0)
                    {
                        yield return new ValidationResult(
                            $"La serie {j + 1} del ejercicio {i + 1} necesita repeticiones.",
                            new[] { nameof(Ejercicios) });
                    }

                    if (EsAutoCarga)
                    {
                        if (serie.EsUnoRm)
                        {
                            yield return new ValidationResult(
                                "En auto carga no se marca 1RM.",
                                new[] { nameof(Ejercicios) });
                        }

                        continue;
                    }

                    if (serie.PesoKg is < 0)
                    {
                        yield return new ValidationResult(
                            $"La serie {j + 1} del ejercicio {i + 1} tiene un peso inválido.",
                            new[] { nameof(Ejercicios) });
                    }

                    if (serie.EsUnoRm)
                    {
                        marcadas++;
                        if (serie.PesoKg is null or <= 0)
                        {
                            yield return new ValidationResult(
                                $"La serie marcada como 1RM del ejercicio {i + 1} necesita un peso.",
                                new[] { nameof(Ejercicios) });
                        }
                    }
                }

                if (!EsAutoCarga && marcadas > 1)
                {
                    yield return new ValidationResult(
                        $"El ejercicio {i + 1} solo puede tener una serie marcada como 1RM.",
                        new[] { nameof(Ejercicios) });
                }
            }
        }
    }

    public class GuardarControlEjercicioRequest
    {
        [Required(ErrorMessage = "El ejercicio necesita un nombre."), MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        public List<GuardarControlSerieRequest> Series { get; set; } = [];
    }

    public class GuardarControlSerieRequest
    {
        [Range(1, 999, ErrorMessage = "Las repeticiones deben ser al menos 1.")]
        public int Repeticiones { get; set; }

        [Range(0, 9999, ErrorMessage = "El peso no es válido.")]
        public decimal? PesoKg { get; set; }

        public bool EsUnoRm { get; set; }
    }
}
