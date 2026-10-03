using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Nota o anotacion del entrenador. Puede cargarse a mano o generarse
    /// a partir de un audio que se transcribe.
    /// </summary>
    public class Anotacion
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Titulo { get; set; }

        /// <summary>Texto final de la anotacion (editable por el entrenador).</summary>
        [Required]
        public string Texto { get; set; } = string.Empty;

        /// <summary>
        /// Salida cruda del motor de transcripcion, tal cual la devolvio.
        /// Se conserva para poder comparar o reprocesar.
        /// </summary>
        public string? TranscripcionOriginal { get; set; }

        public OrigenAnotacion Origen { get; set; } = OrigenAnotacion.Manual;

        // --------------------- Datos del audio ---------------------

        /// <summary>Ruta relativa del audio guardado.</summary>
        [MaxLength(500)]
        public string? AudioRuta { get; set; }

        [MaxLength(100)]
        public string? AudioContentType { get; set; }

        public int? AudioDuracionSegundos { get; set; }

        public long? AudioTamanioBytes { get; set; }

        /// <summary>Proveedor usado para transcribir (ej: "deepgram", "openai").</summary>
        [MaxLength(50)]
        public string? ProveedorTranscripcion { get; set; }

        [MaxLength(50)]
        public string? ModeloTranscripcion { get; set; }

        [MaxLength(10)]
        public string? Idioma { get; set; }

        /// <summary>Confianza promedio informada por el proveedor (0 a 1).</summary>
        public double? ConfianzaPromedio { get; set; }

        /// <summary>
        /// true cuando la transcripcion es dudosa (por ejemplo, mucho ruido de viento)
        /// y conviene que el entrenador la revise.
        /// </summary>
        public bool RequiereRevision { get; set; }

        // --------------------- Relaciones (opcionales) ---------------------

        public int? AtletaId { get; set; }
        public Atleta? Atleta { get; set; }

        public int? EntrenamientoId { get; set; }
        public Entrenamiento? Entrenamiento { get; set; }

        /// <summary>
        /// Trabajo sobre el que se escribio la anotacion. Es opcional: una nota
        /// puede ser general, de la sesion, del atleta o de un trabajo puntual.
        /// </summary>
        public int? TrabajoId { get; set; }
        public Trabajo? Trabajo { get; set; }

        /// <summary>Usuario que cargo la anotacion.</summary>
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
