using SistemaVenta.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.Domain.Tests.Entities
{
    public class CategoriaTests
    {
        [Fact]
        public void CrearCategoria_ConDatosValidos_DebeCrearseActiva()
        {
            
            string nombre = "Monitores";
            string descripcion = "Monitores y pantallas";

            
            var categoria = new Categoria(nombre, descripcion);

            
            Assert.Equal("Monitores", categoria.Nombre);
            Assert.Equal(
                "Monitores y pantallas",
                categoria.Descripcion);

            Assert.True(categoria.Activa);
        }

        [Fact]
        public void CrearCategoria_SinNombre_DebeLanzarExcepcion()
        {
            Assert.Throws<ArgumentException>(() =>
                new Categoria(""));
        }

        [Fact]
        public void CrearCategoria_ConEspaciosEnNombre_DebeEliminarEspaciosExternos()
        {
            var categoria = new Categoria(
                "   Monitores   ");

            Assert.Equal("Monitores", categoria.Nombre);
        }
    }
}
