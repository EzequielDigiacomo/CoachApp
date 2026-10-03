using System.Globalization;

namespace Entidades.Util
{
    /// <summary>
    /// Convierte tiempos y horas entre el texto que escribe el entrenador
    /// ("4:30", "1:02:03.5", "09:30") y los tipos que se guardan.
    /// El formato de entrada es libre: segundos, m:ss o h:mm:ss.
    /// </summary>
    public static class TiempoTexto
    {
        /// <summary>
        /// Pasa un TimeSpan a texto. Menos de una hora como "m:ss" (4:30)
        /// y de una hora en adelante como "h:mm:ss" (1:02:03).
        /// Las centesimas se muestran solo si existen.
        /// </summary>
        public static string Formatear(TimeSpan tiempo)
        {
            var centesimas = (long)Math.Round(tiempo.TotalSeconds * 100, MidpointRounding.AwayFromZero);

            var horas = centesimas / 360000;
            var minutos = centesimas % 360000 / 6000;
            var segundos = centesimas % 6000 / 100;
            var fraccion = centesimas % 100;

            var sufijo = fraccion > 0 ? $".{fraccion:00}".TrimEnd('0') : string.Empty;

            return horas > 0
                ? $"{horas}:{minutos:00}:{segundos:00}{sufijo}"
                : $"{minutos}:{segundos:00}{sufijo}";
        }

        /// <summary>
        /// Lee un tiempo escrito a mano. Acepta "45" (segundos), "4:30"
        /// (minutos y segundos) y "1:02:03,5" (horas, minutos y segundos).
        /// </summary>
        public static bool IntentaParse(string? texto, out TimeSpan tiempo)
        {
            tiempo = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            // La coma decimal es lo que sale natural al escribirlo a mano.
            var partes = texto.Trim().Replace(',', '.').Split(':');
            if (partes.Length is 0 or > 3)
            {
                return false;
            }

            if (!double.TryParse(partes[^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var ultima) || ultima < 0)
            {
                return false;
            }

            var minutosTotales = 0;
            if (partes.Length == 3)
            {
                if (!int.TryParse(partes[0], out var horas) || horas < 0)
                {
                    return false;
                }

                if (!int.TryParse(partes[1], out var minutos) || minutos is < 0 or > 59)
                {
                    return false;
                }

                minutosTotales = horas * 60 + minutos;
            }
            else if (partes.Length == 2)
            {
                if (!int.TryParse(partes[0], out var minutos) || minutos < 0)
                {
                    return false;
                }

                minutosTotales = minutos;
            }

            // Los segundos de un tiempo compuesto no pueden llegar a un minuto.
            if (partes.Length > 1 && ultima >= 60)
            {
                return false;
            }

            tiempo = TimeSpan.FromSeconds(minutosTotales * 60 + ultima);
            return true;
        }

        /// <summary>Pasa una hora del dia a texto "HH:mm".</summary>
        public static string FormatearHora(TimeOnly hora) =>
            hora.ToString("HH:mm", CultureInfo.InvariantCulture);

        /// <summary>Lee una hora del dia escrita como "09:30" o "9:30".</summary>
        public static bool IntentaParseHora(string? texto, out TimeOnly hora) =>
            TimeOnly.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.None, out hora);
    }
}
