using Entidades.Util;

namespace CoachApp.Tests.Util;

public class PaginacionTests
{
    [Fact]
    public void Normalizar_defaults()
    {
        var (skip, take, pagina, tamano) = Paginacion.Normalizar(null, null);
        Assert.Equal(0, skip);
        Assert.Equal(Paginacion.TamanoDefault, take);
        Assert.Equal(1, pagina);
        Assert.Equal(Paginacion.TamanoDefault, tamano);
    }

    [Fact]
    public void Normalizar_respeta_pagina_y_tope()
    {
        var (skip, take, pagina, tamano) = Paginacion.Normalizar(3, 9999);
        Assert.Equal(2 * Paginacion.TamanoMaximo, skip);
        Assert.Equal(Paginacion.TamanoMaximo, take);
        Assert.Equal(3, pagina);
        Assert.Equal(Paginacion.TamanoMaximo, tamano);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-5, -1, 1, Paginacion.TamanoDefault)]
    public void Normalizar_valores_invalidos(int paginaIn, int tamanoIn, int paginaEsperada, int tamanoEsperado)
    {
        var (skip, take, pagina, tamano) = Paginacion.Normalizar(paginaIn, tamanoIn);
        Assert.Equal((paginaEsperada - 1) * tamanoEsperado, skip);
        Assert.Equal(tamanoEsperado, take);
        Assert.Equal(paginaEsperada, pagina);
        Assert.Equal(tamanoEsperado, tamano);
    }
}
