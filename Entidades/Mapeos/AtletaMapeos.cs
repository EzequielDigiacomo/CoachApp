using Entidades.DTOs;

namespace Entidades.Mapeos
{
    /// <summary>Conversion de la entidad Atleta a su DTO publico.</summary>
    public static class AtletaMapeos
    {
        /// <summary>
        /// Arma el DTO del atleta y calcula la edad y la condicion de menor,
        /// que no se guardan en la base sino que dependen de la fecha de hoy.
        /// </summary>
        public static AtletaDto ToDto(this Atleta atleta)
        {
            var edad = CalcularEdad(atleta.FechaNacimiento);

            return new AtletaDto
            {
                Id = atleta.Id,
                Nombre = atleta.Nombre,
                Apellido = atleta.Apellido,
                FechaNacimiento = atleta.FechaNacimiento,
                Edad = edad,
                EsMenor = edad < 18,
                Club = atleta.Club,
                Email = atleta.Email,
                Dni = atleta.Dni,
                Telefono = atleta.Telefono,
                Activo = atleta.Activo,
                FechaAlta = atleta.FechaAlta
            };
        }

        /// <summary>
        /// Edad en anios cumplidos a la fecha de hoy.
        /// </summary>
        public static int CalcularEdad(DateOnly fechaNacimiento)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var edad = hoy.Year - fechaNacimiento.Year;

            // Todavia no cumplio anios este anio.
            if (fechaNacimiento > hoy.AddYears(-edad))
            {
                edad--;
            }

            return Math.Max(edad, 0);
        }
    }
}
