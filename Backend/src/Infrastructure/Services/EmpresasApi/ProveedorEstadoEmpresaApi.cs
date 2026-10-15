using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private readonly IProveedorTokenDatosMaestros _proveedorToken;

        public ProveedorEstadoEmpresaApi(
            HttpClient httpClient,
            IProveedorTokenDatosMaestros proveedorToken
        )
        {
            _httpClient =
                httpClient ??
                throw new ArgumentNullException(
                    nameof(httpClient)
                );

            _proveedorToken =
                proveedorToken ??
                throw new ArgumentNullException(
                    nameof(proveedorToken)
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
                $"empresas/estado/" +
                $"{cuitEmpresa}";

            HttpResponseMessage response =
                await EnviarAsync(
                    url,
                    cancellationToken
                );

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                _proveedorToken.InvalidarToken();

                response = await EnviarAsync(
                    url,
                    cancellationToken
                );
            }

            using (response)
            {
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

        private async Task<HttpResponseMessage> EnviarAsync(
            string ruta,
            CancellationToken cancellationToken
        )
        {
            string token = await _proveedorToken.ObtenerTokenAsync(
                cancellationToken
            );

            using HttpRequestMessage solicitud = new HttpRequestMessage(
                HttpMethod.Get,
                ruta
            );

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            solicitud.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            return await _httpClient.SendAsync(
                solicitud,
                cancellationToken
            );
        }
    }
}