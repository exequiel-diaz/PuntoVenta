
using SistemaVenta.Application.Interfaces;
using SistemaVenta.Domain.Entities;
using SistemaVenta.Domain.Enums;

namespace SistemaVenta.Application.UseCases.Pagos
{
    public class RegistrarPago
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarPago(
            IPagoRepository pagoRepository,
            IUnitOfWork unitOfWork)
        {
            _pagoRepository = pagoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> EjecutarAsync(
            decimal importe,
            FormaPago formaPago,
            int usuarioId,
            string? observaciones = null,
            CancellationToken cancellationToken = default)
        {
            var pago = new Pago(
                importe,
                formaPago,
                usuarioId,
                observaciones);

            await _pagoRepository.AgregarAsync(
                pago,
                cancellationToken);

            await _unitOfWork.GuardarCambiosAsync();

            return pago.Id;
        }
    }
}
