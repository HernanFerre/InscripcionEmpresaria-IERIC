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
    public class AddEmpresaCommandHandler : IRequestHandler<AddEmpresaCommand, Guid>
    {
        private readonly IEmpresaRepository _EmpresasRepository;

        public AddEmpresaCommandHandler(IEmpresaRepository EmpresaRepository)
        {
            _EmpresasRepository = EmpresaRepository;
        }

        public async Task<Guid> Handle(AddEmpresaCommand command, CancellationToken cancellationToken)
        {
            Empresa empresaNueva = new Empresa((Cuit)command.CUIT, command.RazonSocial, command.EsCooperativa);
            _EmpresasRepository.Add(empresaNueva);
            await _EmpresasRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return empresaNueva.Id;
        }
    }
}