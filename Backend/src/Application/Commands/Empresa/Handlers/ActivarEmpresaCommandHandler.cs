using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using IERIC.SumariosIERIC.Domain.Enums;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Domain.Exceptions;

namespace IERIC.SumariosIERIC.Application.Commands
{
    // Regular CommandHandler
    public class ActivarEmpresaCommandHandler : IRequestHandler<ActivarEmpresaCommand, Guid>
    {
        private readonly IEmpresaRepository _EmpresasRepository;

        public ActivarEmpresaCommandHandler(IEmpresaRepository EmpresaRepository)
        {
            _EmpresasRepository = EmpresaRepository;
        }

        public async Task<Guid> Handle(ActivarEmpresaCommand command, CancellationToken cancellationToken)
        {
            Empresa empresa = await _EmpresasRepository.GetById(command.EmpresaId);
            if (empresa == null) throw new SumariosDomainException("No se encontró la empresa");
            _EmpresasRepository.ActivarEmpresa(empresa);
            await _EmpresasRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return empresa.Id;
        }
    }
}