using System;
using System.Text.Json.Serialization;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Events;

namespace IERIC.SumariosIERIC.Application.IntegrationEvents
{
    public record EmpresaModificadaIntegrationEvent : IntegrationEvent
    {
        [JsonInclude]
        public Guid EmpresaId { get; set; }
        [JsonConstructor]
        public EmpresaModificadaIntegrationEvent(Guid empresaId)
        {
            EmpresaId = empresaId;

        }
    }
}