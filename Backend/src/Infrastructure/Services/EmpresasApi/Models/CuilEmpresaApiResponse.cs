using System;
using System.Text.Json.Serialization;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    internal sealed class CuilEmpresaApiResponse
    {
        [JsonPropertyName("nombres")]
        public string Nombres { get; set; }

        [JsonPropertyName("apellidos")]
        public string Apellidos { get; set; }

        [JsonPropertyName("cuil")]
        public long Cuil { get; set; }

        [JsonPropertyName("fechaIngreso")]
        public DateTime FechaIngreso { get; set; }

        [JsonPropertyName("fechaEgreso")]
        public DateTime? FechaEgreso { get; set; }
    }
}