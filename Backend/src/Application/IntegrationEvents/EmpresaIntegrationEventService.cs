using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Abstractions;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Events;
using IERIC.SumariosIERIC.BuildingBlocks.IntegrationEventLogEF;
using IERIC.SumariosIERIC.BuildingBlocks.IntegrationEventLogEF.Services;
using IERIC.SumariosIERIC.Infrastructure;

namespace IERIC.SumariosIERIC.Application.IntegrationEvents
{
    public class EmpresaIntegrationEventService : IEmpresaIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly SumariosContext _SumariosContext;
        private readonly IIntegrationEventLogService _eventLogService;
        private readonly ILogger<EmpresaIntegrationEventService> _logger;

        public EmpresaIntegrationEventService(IEventBus eventBus,
            SumariosContext catalogoMaterialesContext,
            IntegrationEventLogContext eventLogContext,
            Func<DbConnection, IIntegrationEventLogService> integrationEventLogServiceFactory,
            ILogger<EmpresaIntegrationEventService> logger)
        {
            _SumariosContext = catalogoMaterialesContext ?? throw new ArgumentNullException(nameof(catalogoMaterialesContext));
            _integrationEventLogServiceFactory = integrationEventLogServiceFactory ?? throw new ArgumentNullException(nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(_SumariosContext.Database.GetDbConnection());
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
        {
            var pendingLogEvents = await _eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

            foreach (var logEvt in pendingLogEvents)
            {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", logEvt.EventId, Program.AppName, logEvt.IntegrationEvent);

                try
                {
                    await _eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);
                    _eventBus.Publish(logEvt.IntegrationEvent);
                    await _eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ERROR publishing integration event: {IntegrationEventId} from {AppName}", logEvt.EventId, Program.AppName);

                    await _eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
                }
            }
        }

        public async Task AddAndSaveEventAsync(IntegrationEvent evt, Guid transactionId)
        {
            _logger.LogInformation("----- Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", evt.Id, evt);

            //IDbContextTransaction transactionId = _catalogoMaterialesContext.GetCurrentTransaction();

            await _eventLogService.SaveEventAsync(evt, transactionId);

        }


    }
}