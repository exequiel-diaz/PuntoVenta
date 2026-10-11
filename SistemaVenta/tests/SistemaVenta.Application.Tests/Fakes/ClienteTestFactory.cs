using System.Reflection;
using SistemaVenta.Domain.Entities;

namespace SistemaVenta.Application.Tests.Fakes
{
    public static class ClienteTestFactory
    {
        public static T ConId<T>(T cliente, int id)
            where T : Cliente
        {
            var propiedad = typeof(Cliente).GetProperty(
                nameof(Cliente.Id),
                BindingFlags.Instance | BindingFlags.Public);

            if (propiedad is null)
                throw new InvalidOperationException(
                    "No se encontró la propiedad Id.");

            propiedad.SetValue(cliente, id);

            return cliente;
        }
    }
}
