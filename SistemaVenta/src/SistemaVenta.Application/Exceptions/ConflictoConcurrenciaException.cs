
namespace SistemaVenta.Application.Exceptions;

public class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException()
        : base("Los datos fueron modificados por otra operación. Actualizá la información e intentá nuevamente.")
    {
    }

    public ConflictoConcurrenciaException(
        string mensaje,
        Exception excepcionInterna)
        : base(mensaje, excepcionInterna)
    {
    }
}
