using System.Text.Json.Serialization;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    internal sealed class CuilEmpresaApiResponse
    {
        [JsonPropertyName("cuil")]
        public long Cuil { get; set; }
    }
}