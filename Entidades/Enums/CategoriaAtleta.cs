using System.ComponentModel.DataAnnotations;

namespace Entidades.Enums
{
    /// <summary>
    /// Categoria segun la edad que el atleta cumple en el anio en curso.
    /// Menor es 13 y 14, cadete 15 y 16, junior 17 y 18.
    /// </summary>
    public enum CategoriaAtleta
    {
        [Display(Name = "Pre-infantil (8-10 años)")]
        Preinfantil = 1,

        [Display(Name = "Infantil (11-12 años)")]
        Infantil = 2,

        [Display(Name = "Menor (13-14 años)")]
        Menor = 3,

        [Display(Name = "Cadete (15-16 años)")]
        Cadete = 4,

        [Display(Name = "Junior (17-18 años)")]
        Junior = 5,

        [Display(Name = "Sub-23 (19-22 años)")]
        Sub23 = 6,

        [Display(Name = "Senior (23-35 años)")]
        Senior = 7,

        [Display(Name = "Master A (40-45 años)")]
        MasterA = 8,

        [Display(Name = "Master B (46-49 años)")]
        MasterB = 9,

        [Display(Name = "Master C (50+ años)")]
        MasterC = 10,
    }
}
