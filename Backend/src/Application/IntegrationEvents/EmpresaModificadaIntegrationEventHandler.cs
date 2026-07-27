using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Abstractions;
using IERIC.SumariosIERIC.Domain.Entities;


namespace IERIC.SumariosIERIC.Application.IntegrationEvents
{
    public class EmpresaModificadaIntegrationEventHandler : IIntegrationEventHandler<EmpresaModificadaIntegrationEvent>
    {
        private readonly ILogger<EmpresaModificadaIntegrationEventHandler> _logger;
        private readonly IEmpresaRepository _repository;

        public EmpresaModificadaIntegrationEventHandler(
            ILogger<EmpresaModificadaIntegrationEventHandler> logger,
            IEmpresaRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(EmpresaModificadaIntegrationEvent @event)
        {
            _logger.LogInformation("Se Modifico el Empresa Id = " + @event.EmpresaId.ToString());
        }
    }


}