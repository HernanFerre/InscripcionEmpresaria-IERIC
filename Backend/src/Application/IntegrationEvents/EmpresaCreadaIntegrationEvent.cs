using System;
using System.Text.Json.Serialization;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Events;

namespace IERIC.SumariosIERIC.Application.IntegrationEvents
{
    public record EmpresaCreadaIntegrationEvent : IntegrationEvent
    {
        [JsonInclude]
        public Guid MaterialId { get; set; }

        [JsonConstructor]
        public EmpresaCreadaIntegrationEvent(Guid materialId)
        {
            MaterialId = materialId;

        }
    }
}