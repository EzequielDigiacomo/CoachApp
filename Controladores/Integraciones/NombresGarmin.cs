using System.Globalization;
using System.Text;

namespace Controladores.Integraciones
{
    /// <summary>
    /// Compara el atleta con el amigo de Garmin. Ignora mayusculas y tildes.
    /// Alcanza con el nombre: si Garmin muestra solo "Brahian", coincide con
    /// un atleta llamado Brahian aunque el apellido sea otro.
    /// </summary>
    public static class NombresGarmin
    {
        public static bool Coincide(string? nombre, string? apellido, string? nombreGarmin)
        {
            var garmin = Palabras(nombreGarmin);
            var soloNombre = Palabras(nombre);
            if (garmin.Length == 0 || soloNombre.Length == 0)
            {
                return false;
            }

            var garminTexto = string.Join(' ', garmin);
            var nombreTexto = string.Join(' ', soloNombre);
            if (garminTexto == nombreTexto
                || garminTexto.StartsWith(nombreTexto + " ", StringComparison.Ordinal)
                || nombreTexto.StartsWith(garminTexto + " ", StringComparison.Ordinal))
            {
                return true;
            }

            var completo = Palabras(nombre).Concat(Palabras(apellido)).Order(StringComparer.Ordinal).ToArray();
            var garminOrdenado = garmin.Order(StringComparer.Ordinal).ToArray();
            return completo.SequenceEqual(garminOrdenado);
        }

        /// <summary>Palabras ya normalizadas, en el orden original.</summary>
        private static string[] Palabras(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return [];
            }

            return Normalizar(texto).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string Normalizar(string texto)
        {
            var descompuesto = texto.Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                limpio.Append(char.IsLetterOrDigit(caracter) ? char.ToLowerInvariant(caracter) : ' ');
            }

            return limpio.ToString();
        }
    }
}
