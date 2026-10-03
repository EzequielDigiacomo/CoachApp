using System.IO.Compression;
using System.Xml.Linq;

namespace Controladores.Integraciones
{
    /// <summary>
    /// Lee un libro de Excel (.xlsx) en memoria. Un xlsx es un zip con XML
    /// adentro, asi que se puede leer con lo que ya trae .NET, sin librerias
    /// nuevas.
    ///
    /// Se lee el xlsx y no el CSV a proposito: se comprobo que Google devuelve
    /// el CSV vacio para libros subidos desde Excel con muchas pestañas, y el
    /// xlsx del mismo libro trae todo. Ademas el xlsx es lo unico que expone
    /// los nombres de las pestañas, que es la lista que ve el entrenador.
    /// </summary>
    public static class LectorXlsx
    {
        private static readonly XNamespace Ns =
            "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        private const string NsRelaciones =
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        /// <summary>Tope de filas y columnas para no devolver una tabla inmanejable.</summary>
        public const int MaximoFilas = 1000;
        public const int MaximoColumnas = 60;

        /// <summary>Tope de celdas en total, por si una pestaña viene muy cargada.</summary>
        private const int MaximoCeldas = 30_000;

        /// <summary>Un problema al leer el libro, con un mensaje mostrable.</summary>
        public class LibroInvalidoException : Exception
        {
            public LibroInvalidoException(string mensaje) : base(mensaje)
            {
            }
        }

        /// <summary>Los nombres de las pestañas, en el orden en que se ven en Excel.</summary>
        public static List<string> NombresDePestanas(byte[] libro)
        {
            using var zip = Abrir(libro);
            return Pestanas(zip).Nombres;
        }

        /// <summary>Lo que se leyo de una pestaña.</summary>
        public sealed record PestanaLeida(
            int Columnas,
            int ColumnaInicial,
            List<(int Numero, List<string> Celdas)> Filas);

        /// <summary>
        /// Devuelve el contenido de una pestaña, opcionalmente recortado a un
        /// rango. Cada fila viene con su numero real en la planilla.
        /// </summary>
        public static PestanaLeida LeerPestana(byte[] libro, string nombre, RangoHoja? rango = null)
        {
            using var zip = Abrir(libro);

            var (nombres, archivos) = Pestanas(zip);

            var indice = nombres.FindIndex(n => string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase));
            if (indice < 0)
            {
                throw new LibroInvalidoException(
                    $"La pestaña «{nombre}» ya no está en el libro. Volvé a leer la planilla.");
            }

            var compartidos = TextosCompartidos(zip);
            return LeerHoja(zip, archivos[indice], compartidos, rango);
        }

        private static ZipArchive Abrir(byte[] libro)
        {
            // Un xlsx siempre arranca con la firma de zip, "PK".
            if (libro.Length < 2 || libro[0] != (byte)'P' || libro[1] != (byte)'K')
            {
                throw new LibroInvalidoException("Google no devolvió un libro de Excel legible.");
            }

            return new ZipArchive(new MemoryStream(libro), ZipArchiveMode.Read);
        }

        private static ZipArchiveEntry? Entrada(ZipArchive zip, string ruta)
        {
            var buscada = ruta.TrimStart('/');

            return zip.Entries.FirstOrDefault(e =>
                string.Equals(e.FullName.TrimStart('/'), buscada, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Nombres de las pestañas y el archivo XML de cada una.</summary>
        private static (List<string> Nombres, List<string> Archivos) Pestanas(ZipArchive zip)
        {
            var entrada = Entrada(zip, "xl/workbook.xml")
                ?? throw new LibroInvalidoException("El libro no tiene la parte principal (xl/workbook.xml).");

            var relaciones = Relaciones(zip);

            var nombres = new List<string>();
            var archivos = new List<string>();

            XDocument documento;
            using (var stream = entrada.Open())
            {
                documento = XDocument.Load(stream);
            }

            foreach (var hoja in documento.Descendants(Ns + "sheet"))
            {
                var nombre = (string?)hoja.Attribute("name");
                var id = (string?)hoja.Attribute(XNamespace.Get(NsRelaciones) + "id");

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(id))
                {
                    continue;
                }

                if (!relaciones.TryGetValue(id, out var destino))
                {
                    continue;
                }

                nombres.Add(nombre);
                archivos.Add(destino);
            }

            if (nombres.Count == 0)
            {
                throw new LibroInvalidoException("No se encontraron pestañas en el libro.");
            }

            return (nombres, archivos);
        }

        /// <summary>Relacion id -> archivo dentro del zip, del libro de trabajo.</summary>
        private static Dictionary<string, string> Relaciones(ZipArchive zip)
        {
            var mapa = new Dictionary<string, string>();

            var entrada = Entrada(zip, "xl/_rels/workbook.xml.rels");
            if (entrada is null)
            {
                return mapa;
            }

            XDocument documento;
            using (var stream = entrada.Open())
            {
                documento = XDocument.Load(stream);
            }

            foreach (var relacion in documento.Root?.Elements() ?? [])
            {
                var id = (string?)relacion.Attribute("Id");
                var destino = (string?)relacion.Attribute("Target");

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(destino))
                {
                    continue;
                }

                // El destino viene relativo a xl/ (o absoluto dentro del zip).
                var ruta = destino.StartsWith('/')
                    ? destino.TrimStart('/')
                    : "xl/" + destino;

                mapa[id] = Normalizar(ruta);
            }

            return mapa;
        }

        /// <summary>Resuelve los ".." que puede traer un destino del zip.</summary>
        private static string Normalizar(string ruta)
        {
            var partes = new List<string>();

            foreach (var parte in ruta.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (parte == ".")
                {
                    continue;
                }

                if (parte == "..")
                {
                    if (partes.Count > 0)
                    {
                        partes.RemoveAt(partes.Count - 1);
                    }

                    continue;
                }

                partes.Add(parte);
            }

            return string.Join('/', partes);
        }

        /// <summary>
        /// La tabla de textos del libro. Las celdas de texto no guardan el
        /// texto: guardan un numero que apunta aca.
        /// </summary>
        private static List<string> TextosCompartidos(ZipArchive zip)
        {
            var textos = new List<string>();

            var entrada = Entrada(zip, "xl/sharedStrings.xml");
            if (entrada is null)
            {
                return textos;
            }

            XDocument documento;
            using (var stream = entrada.Open())
            {
                documento = XDocument.Load(stream);
            }

            foreach (var item in documento.Descendants(Ns + "si"))
            {
                // Un texto puede venir en varios pedazos con formato distinto;
                // se concatenan todos. Se saltean los <t> de pronunciacion, que
                // no son parte del texto.
                var partes = item
                    .Descendants(Ns + "t")
                    .Where(t => t.Parent?.Name != Ns + "rPh")
                    .Select(t => t.Value);

                textos.Add(string.Concat(partes));
            }

            return textos;
        }

        private static PestanaLeida LeerHoja(
            ZipArchive zip, string archivo, List<string> compartidos, RangoHoja? rango)
        {
            var filas = new List<(int, List<string>)>();
            var totalCeldas = 0;

            // Con un rango pedido, el ancho de la tabla es fijo; sin rango, es
            // el de la columna mas lejana que tenga algo.
            var conRango = rango is not null;
            var primeraColumna = rango?.PrimeraColumna ?? 1;
            var ancho = conRango ? rango!.UltimaColumna - rango.PrimeraColumna + 1 : 0;

            var entrada = Entrada(zip, archivo);
            if (entrada is null)
            {
                return new PestanaLeida(0, primeraColumna, filas);
            }

            XDocument documento;
            using (var stream = entrada.Open())
            {
                documento = XDocument.Load(stream);
            }

            // Una pestaña de solo graficos no tiene datos: se devuelve vacia.
            var datos = documento.Root?.Element(Ns + "sheetData");
            if (datos is null)
            {
                return new PestanaLeida(0, primeraColumna, filas);
            }

            var numero = 0;

            foreach (var fila in datos.Elements(Ns + "row"))
            {
                numero = (int?)fila.Attribute("r") ?? numero + 1;

                if (rango is not null)
                {
                    if (numero < rango.PrimeraFila)
                    {
                        continue;
                    }

                    if (numero > rango.UltimaFila)
                    {
                        break;
                    }
                }

                var celdas = new Dictionary<int, string>();
                var columna = 0;

                foreach (var celda in fila.Elements(Ns + "c"))
                {
                    columna = ColumnaDe((string?)celda.Attribute("r")) ?? columna + 1;

                    // Fuera del rango pedido: no interesa.
                    if (rango is not null && (columna < rango.PrimeraColumna || columna > rango.UltimaColumna))
                    {
                        continue;
                    }

                    var valor = Valor(celda, compartidos);
                    if (valor.Length > 0)
                    {
                        celdas[columna] = valor;
                    }
                }

                // Sin rango, las filas vacias se saltean: una planilla suele
                // tener miles al final y no aportan nada. Con un rango pedido a
                // proposito, se muestran igual: mantienen la numeracion y el
                // aspecto de la hoja.
                if (celdas.Count == 0 && !conRango)
                {
                    continue;
                }

                if (!conRango && celdas.Count > 0)
                {
                    var maxima = celdas.Keys.Max();
                    if (maxima > ancho)
                    {
                        ancho = Math.Min(maxima, MaximoColumnas);
                    }
                }

                totalCeldas += celdas.Count;
                if (filas.Count >= MaximoFilas || totalCeldas > MaximoCeldas)
                {
                    break;
                }

                var valores = new List<string>(ancho);
                for (var i = primeraColumna; i < primeraColumna + ancho; i++)
                {
                    valores.Add(celdas.TryGetValue(i, out var texto) ? texto : string.Empty);
                }

                filas.Add((numero, valores));
            }

            // Todas las filas con el mismo ancho, para que la tabla cierre.
            foreach (var (_, valores) in filas)
            {
                while (valores.Count < ancho)
                {
                    valores.Add(string.Empty);
                }
            }

            return new PestanaLeida(ancho, primeraColumna, filas);
        }

        private static string Valor(XElement celda, List<string> compartidos)
        {
            var tipo = (string?)celda.Attribute("t");

            // Texto escrito directo en la celda.
            if (tipo == "inlineStr")
            {
                var enLinea = celda.Element(Ns + "is");
                return enLinea is null
                    ? string.Empty
                    : string.Concat(enLinea.Descendants(Ns + "t").Select(t => t.Value));
            }

            var valor = celda.Element(Ns + "v");
            if (valor is null)
            {
                return string.Empty;
            }

            var texto = valor.Value;

            // "s" significa: el texto esta en la tabla de textos compartidos.
            if (tipo == "s" && int.TryParse(texto, out var indice)
                && indice >= 0 && indice < compartidos.Count)
            {
                return compartidos[indice];
            }

            return texto;
        }

        /// <summary>Convierte "AB12" en el numero de columna 28.</summary>
        private static int? ColumnaDe(string? referencia)
        {
            if (string.IsNullOrEmpty(referencia))
            {
                return null;
            }

            var numero = 0;
            var letras = 0;

            foreach (var caracter in referencia)
            {
                if (caracter is >= 'A' and <= 'Z')
                {
                    numero = numero * 26 + (caracter - 'A' + 1);
                    letras++;
                    continue;
                }

                break;
            }

            return letras == 0 ? null : numero;
        }
    }
}
