namespace Entidades.DTOs
{
    /// <summary>
    /// Una pestaña de la planilla ya leida. El texto viene tal como esta
    /// escrito en la hoja: no se interpreta ni se reformatea nada.
    /// </summary>
    public class HojaLeidaDto
    {
        /// <summary>Nombre de la pestaña que se leyo.</summary>
        public string Pestana { get; set; } = string.Empty;

        /// <summary>Cantidad de columnas de la tabla.</summary>
        public int Columnas { get; set; }

        /// <summary>
        /// Numero de la primera columna (1 = A). Es lo que permite rotular la
        /// tabla con las letras reales cuando se leyo un rango que no arranca
        /// en A.
        /// </summary>
        public int ColumnaInicial { get; set; } = 1;

        /// <summary>Las filas con contenido, sin contar las vacias.</summary>
        public List<FilaHojaDto> Filas { get; set; } = [];
    }

    /// <summary>Una fila de la planilla, con el numero que ocupa en la hoja.</summary>
    public class FilaHojaDto
    {
        /// <summary>Numero de fila en la planilla, tal como se ve en Excel.</summary>
        public int Numero { get; set; }

        /// <summary>El texto de cada celda, tal cual esta escrito.</summary>
        public List<string> Celdas { get; set; } = [];
    }
}
