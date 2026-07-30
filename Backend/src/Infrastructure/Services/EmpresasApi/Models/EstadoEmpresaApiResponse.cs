using System.Text.Json.Serialization;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    internal sealed class EstadoEmpresaApiResponse
    {
        [JsonPropertyName("cuit")]
        public long Cuit { get; set; }

        [JsonPropertyName("razon")]
        public string RazonSocial { get; set; }

        [JsonPropertyName("codigoestado")]
        public int CodigoEstado { get; set; }

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; }
    }
}