namespace Controladores.Integraciones
{
    /// <summary>
    /// El recorte de una pestaña: "B4:E49". Sirve para que el entrenador vea
    /// solo la parte que le interesa de una planilla enorme, en vez de una
    /// tabla con decenas de columnas.
    /// </summary>
    public sealed record RangoHoja(
        int PrimeraColumna,
        int UltimaColumna,
        int PrimeraFila,
        int UltimaFila)
    {
        /// <summary>
        /// Lee un rango escrito a mano. Devuelve null si no se pidio ninguno,
        /// que significa "la pestaña entera". Lanza <see cref="HojaException"/>
        /// con un mensaje mostrable si no se entiende.
        /// </summary>
        public static RangoHoja? Parsear(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            var limpio = texto
                .Replace("$", string.Empty)
                .Replace(" ", string.Empty)
                .Trim()
                .ToUpperInvariant();

            var partes = limpio.Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length is 0 or > 2)
            {
                throw new HojaException("Las celdas se escriben como B4:E49.");
            }

            var (columnaInicio, filaInicio) = Celda(partes[0]);
            var (columnaFin, filaFin) = partes.Length == 2
                ? Celda(partes[1])
                : (columnaInicio, filaInicio);

            if (columnaInicio > columnaFin || filaInicio > filaFin)
            {
                throw new HojaException("Las celdas están al revés. Escribilas así: B4:E49.");
            }

            if (columnaFin - columnaInicio + 1 > LectorXlsx.MaximoColumnas)
            {
                throw new HojaException(
                    $"Ese rango es muy ancho: hasta {LectorXlsx.MaximoColumnas} columnas.");
            }

            if (filaFin - filaInicio + 1 > LectorXlsx.MaximoFilas)
            {
                throw new HojaException(
                    $"Ese rango es muy alto: hasta {LectorXlsx.MaximoFilas} filas.");
            }

            return new RangoHoja(columnaInicio, columnaFin, filaInicio, filaFin);
        }

        /// <summary>Convierte "B4" en (columna 2, fila 4).</summary>
        private static (int Columna, int Fila) Celda(string celda)
        {
            var letras = 0;
            var columna = 0;

            while (letras < celda.Length && celda[letras] is >= 'A' and <= 'Z')
            {
                columna = columna * 26 + (celda[letras] - 'A' + 1);
                letras++;
            }

            var digitos = celda[letras..];

            if (letras == 0 || !int.TryParse(digitos, out var fila) || fila < 1)
            {
                throw new HojaException($"No entiendo «{celda}». Las celdas se escriben como B4.");
            }

            return (columna, fila);
        }
    }
}
