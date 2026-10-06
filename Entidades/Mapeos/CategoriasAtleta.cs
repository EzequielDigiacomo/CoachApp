using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Entidades.Enums;

namespace Entidades.Mapeos
{
    /// <summary>
    /// Nombre visible de la categoria. Cuenta la edad que cumple en el anio,
    /// no la que ya tiene hoy: quien nace en el segundo semestre sigue en esa
    /// categoria aunque el cumpleanos ya haya pasado.
    /// </summary>
    public static class CategoriasAtleta
    {
        public static List<string> Nombres(DateOnly fechaNacimiento)
        {
            var edad = DateTime.Today.Year - fechaNacimiento.Year;
            if (edad < 0)
            {
                return [];
            }

            foreach (var categoria in Enum.GetValues<CategoriaAtleta>())
            {
                if (Incluye(categoria, edad))
                {
                    return [Nombre(categoria)];
                }
            }

            return [];
        }

        private static bool Incluye(CategoriaAtleta categoria, int edad) => categoria switch
        {
            CategoriaAtleta.Preinfantil => edad is >= 8 and <= 10,
            CategoriaAtleta.Infantil => edad is >= 11 and <= 12,
            CategoriaAtleta.Menor => edad is >= 13 and <= 14,
            CategoriaAtleta.Cadete => edad is >= 15 and <= 16,
            CategoriaAtleta.Junior => edad is >= 17 and <= 18,
            CategoriaAtleta.Sub23 => edad is >= 19 and <= 22,
            CategoriaAtleta.Senior => edad is >= 23 and <= 35,
            CategoriaAtleta.MasterA => edad is >= 40 and <= 45,
            CategoriaAtleta.MasterB => edad is >= 46 and <= 49,
            CategoriaAtleta.MasterC => edad >= 50,
            _ => false
        };

        private static string Nombre(CategoriaAtleta categoria)
        {
            var campo = typeof(CategoriaAtleta).GetField(categoria.ToString());
            var display = campo?.GetCustomAttribute<DisplayAttribute>();
            return display?.GetName() ?? categoria.ToString();
        }
    }
}
