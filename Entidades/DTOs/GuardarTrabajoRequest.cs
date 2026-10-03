using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Datos para crear o modificar un trabajo. Solo lleva el tipo, la hora y
    /// las observaciones: la distancia y el tiempo de cada marca van en los parciales.
    /// Se usa el mismo contrato en los dos casos porque el modal guarda el
    /// trabajo completo con sus parciales.
    /// </summary>
    public class GuardarTrabajoRequest : IValidatableObject
    {
        /// <summary>Lugar donde se hizo el trabajo.</summary>
        [Required(ErrorMessage = "Elegí el tipo de trabajo: gimnasio, tierra o agua.")]
        public TipoTrabajo? Tipo { get; set; }

        /// <summary>Hora del dia en que se hizo el trabajo ("09:30"). Opcional.</summary>
        public string? HoraInicio { get; set; }

        [MaxLength(300, ErrorMessage = "Las observaciones no pueden superar los 300 caracteres.")]
        public string? Observaciones { get; set; }

        /// <summary>Parciales en el orden en que se tomaron con el cronometro.</summary>
        public List<GuardarParcialRequest> Parciales { get; set; } = [];

        /// <summary>Ejercicios con sus series. Se usa en los trabajos de gimnasio.</summary>
        public List<GuardarEjercicioRequest> Ejercicios { get; set; } = [];

        /// <summary>
        /// Muestras de paladas por minuto con su tiempo. Se usa en los trabajos
        /// de agua y se reparten por tiempo en los parciales.
        /// </summary>
        public List<GuardarPaladaRequest> Paladas { get; set; } = [];

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Gimnasio se mide en series y tierra/agua en parciales: cada tipo
            // valida su propia estructura y se ignora la otra.
            if (Tipo == Enums.TipoTrabajo.Gimnasio)
            {
                if (Ejercicios.Count == 0)
                {
                    yield return new ValidationResult(
                        "Agregá al menos un ejercicio.",
                        new[] { nameof(Ejercicios) });
                }

                for (var i = 0; i < Ejercicios.Count; i++)
                {
                    if (Ejercicios[i].Series.Count == 0)
                    {
                        yield return new ValidationResult(
                            $"El ejercicio {i + 1} necesita al menos una serie.",
                            new[] { nameof(Ejercicios) });
                    }
                }
            }
            else
            {
                // Sin parciales el trabajo no tendria ni distancia ni tiempo.
                if (Parciales.Count == 0)
                {
                    yield return new ValidationResult(
                        "Agregá al menos un parcial con su distancia y su tiempo.",
                        new[] { nameof(Parciales) });
                }
            }

            if (!string.IsNullOrWhiteSpace(HoraInicio) && !Util.TiempoTexto.IntentaParseHora(HoraInicio, out _))
            {
                yield return new ValidationResult(
                    "La hora de trabajo no es válida. Escribila como 09:30.",
                    new[] { nameof(HoraInicio) });
            }

            // Los parciales de un trabajo de gimnasio se descartan, asi que no
            // tiene sentido reclamar por su formato.
            if (Tipo == Enums.TipoTrabajo.Gimnasio)
            {
                yield break;
            }

            for (var i = 0; i < Parciales.Count; i++)
            {
                if (!Util.TiempoTexto.IntentaParse(Parciales[i].Tiempo, out _))
                {
                    yield return new ValidationResult(
                        $"El tiempo del parcial {i + 1} no es válido. Escribilo como 2:12 (m:ss).",
                        new[] { nameof(Parciales) });
                }
            }

            // Las paladas son solo de los trabajos de agua: en el resto se ignoran.
            if (Tipo != Enums.TipoTrabajo.Agua)
            {
                yield break;
            }

            for (var i = 0; i < Paladas.Count; i++)
            {
                if (!Util.TiempoTexto.IntentaParse(Paladas[i].Tiempo, out _))
                {
                    yield return new ValidationResult(
                        $"El tiempo de la palada {i + 1} no es válido. Escribilo como 1:40 (m:ss).",
                        new[] { nameof(Paladas) });
                }
            }
        }
    }
}
