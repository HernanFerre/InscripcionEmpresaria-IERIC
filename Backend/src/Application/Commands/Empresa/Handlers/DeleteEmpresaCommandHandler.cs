using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using IERIC.SumariosIERIC.Infrastructure.Repositories;
using IERIC.SumariosIERIC.Domain.Exceptions;

namespace IERIC.SumariosIERIC.Application.Commands
{
    // Regular CommandHandler
    public class DeleteEmpresaCommandHandler : IRequestHandler<DeleteEmpresaCommand, Guid>
    {
        private readonly IEmpresaRepository _EmpresaRepository;

        public DeleteEmpresaCommandHandler(IEmpresaRepository EmpresaRepository)
        {
            _EmpresaRepository = EmpresaRepository;
        }

        public async Task<Guid> Handle(DeleteEmpresaCommand command, CancellationToken cancellationToken)
        {
            Empresa Empresa = await _EmpresaRepository.GetById(command.EmpresaId);
            if (Empresa == null) throw new SumariosDomainException("No se encontró la Empresa");
            _EmpresaRepository.Delete(Empresa);
            await _EmpresaRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return Empresa.Id;
        }
    }
}

