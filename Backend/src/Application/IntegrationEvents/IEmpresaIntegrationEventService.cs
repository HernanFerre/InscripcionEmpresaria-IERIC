using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Events;

namespace IERIC.SumariosIERIC.Application.IntegrationEvents
{

    public interface IEmpresaIntegrationEventService
    {
        Task PublishEventsThroughEventBusAsync(Guid transactionId);
        Task AddAndSaveEventAsync(IntegrationEvent evt, Guid transacationId);
    }
}