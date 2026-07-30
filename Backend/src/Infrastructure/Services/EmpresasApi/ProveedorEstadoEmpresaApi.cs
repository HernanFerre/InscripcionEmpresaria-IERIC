using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    public sealed class ProveedorEstadoEmpresaApi
        : IProveedorEstadoEmpresa
    {
        private readonly HttpClient _httpClient;

        public ProveedorEstadoEmpresaApi(
            HttpClient httpClient
        )
        {
            _httpClient =
                httpClient ??
                throw new ArgumentNullException(
                    nameof(httpClient)
                );
        }

        public async Task<EstadoInscripcionEmpresa>
            ObtenerPorCuitAsync(
                Cuit cuitEmpresa,
                CancellationToken cancellationToken = default
            )
        {
            if (cuitEmpresa == null)
            {
                throw new ArgumentNullException(
                    nameof(cuitEmpresa)
                );
            }

            string url =
                $"empresas/consulta-estado/" +
                $"{cuitEmpresa}";

            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    url,
                    cancellationToken
                );

            response.EnsureSuccessStatusCode();

            EstadoEmpresaApiResponse resultado =
                await response.Content
                    .ReadFromJsonAsync<
                        EstadoEmpresaApiResponse
                    >(
                        cancellationToken:
                            cancellationToken
                    );

            if (resultado == null)
            {
                throw new HttpRequestException(
                    "El servicio de empresas devolvió una respuesta vacía."
                );
            }

            if (
                resultado.Cuit !=
                cuitEmpresa.ToInt64()
            )
            {
                throw new HttpRequestException(
                    "El CUIT devuelto por el servicio no coincide con el CUIT consultado."
                );
            }

            return new EstadoInscripcionEmpresa(
                cuitEmpresa,
                resultado.RazonSocial,
                resultado.CodigoEstado,
                resultado.Mensaje
            );
        }
    }
}