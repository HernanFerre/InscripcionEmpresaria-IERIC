using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Enums;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
namespace IERIC.SumariosIERIC.Domain.Events
{
    public class EmpresaNuevaDomainEventHandler : INotificationHandler<EmpresaNuevaRequested>
    {
        private IEmpresaRepository EmpresaRepository;
        public EmpresaNuevaDomainEventHandler(IEmpresaRepository empresaRepository)
        {
            EmpresaRepository = empresaRepository;
        }

        public async Task Handle(EmpresaNuevaRequested notification, CancellationToken cancellationToken)
        {
            if (await EmpresaRepository.ExistAny(notification.Empresa.Cuit)) throw new SumariosDomainException("La empresa ya se encuentra cargada en el sistema");
        }
    }
}