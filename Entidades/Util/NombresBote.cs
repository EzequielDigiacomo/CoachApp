using Entidades.Enums;

namespace Entidades.Util
{
    /// <summary>Arma el texto visible del bote: "K2 Perez-Gomez".</summary>
    public static class NombresBote
    {
        public static string Etiqueta(TipoBote tipo, IEnumerable<(string Nombre, string Apellido)> miembros)
        {
            var nombres = miembros
                .Select(m => string.IsNullOrWhiteSpace(m.Apellido) ? m.Nombre.Trim() : m.Apellido.Trim())
                .Where(n => n.Length > 0);

            return $"{tipo} {string.Join('-', nombres)}";
        }
    }
}
