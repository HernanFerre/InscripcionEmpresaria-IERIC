using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;

namespace IERIC.SumariosIERIC.Infrastructure.Services.DatosMaestrosApi
{
    public sealed class ProveedorDatosMaestrosApi
        : IProveedorDatosMaestros
    {
        private readonly HttpClient _httpClient;

        private readonly IProveedorTokenDatosMaestros
            _proveedorToken;

        public ProveedorDatosMaestrosApi(
            HttpClient httpClient,
            IProveedorTokenDatosMaestros proveedorToken
        )
        {
            _httpClient = httpClient;
            _proveedorToken = proveedorToken;
        }

        public Task<string>
            ObtenerActividadesConstruccionAsync(
                CancellationToken cancellationToken = default
            )
        {
            return ObtenerAsync(
                "datos-maestros/actividades-construccion",
                cancellationToken
            );
        }

        public Task<string>
            ObtenerCaracteresEmpresaAsync(
                CancellationToken cancellationToken = default
            )
        {
            return ObtenerAsync(
                "datos-maestros/caracteres-empresa",
                cancellationToken
            );
        }

        public Task<string>
            ObtenerTiposSociedadAsync(
                CancellationToken cancellationToken = default
            )
        {
            return ObtenerAsync(
                "datos-maestros/tipos-sociedad",
                cancellationToken
            );
        }

        private async Task<string> ObtenerAsync(
            string ruta,
            CancellationToken cancellationToken
        )
        {
            HttpResponseMessage respuesta =
                await EnviarAsync(
                    ruta,
                    cancellationToken
                );

            if (
                respuesta.StatusCode ==
                HttpStatusCode.Unauthorized
            )
            {
                respuesta.Dispose();

                _proveedorToken.InvalidarToken();

                respuesta =
                    await EnviarAsync(
                        ruta,
                        cancellationToken
                    );
            }

            using (respuesta)
            {
                string contenido =
                    await respuesta.Content
                        .ReadAsStringAsync(
                            cancellationToken
                        );

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        "No fue posible consultar Datos Maestros. " +
                        $"Código HTTP: {(int)respuesta.StatusCode}."
                    );
                }

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    throw new InvalidOperationException(
                        "Datos Maestros devolvió una respuesta vacía."
                    );
                }

                return contenido;
            }
        }

        private async Task<HttpResponseMessage>
            EnviarAsync(
                string ruta,
                CancellationToken cancellationToken
            )
        {
            string token =
                await _proveedorToken.ObtenerTokenAsync(
                    cancellationToken
                );

            using HttpRequestMessage solicitud =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    ruta
                );

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            solicitud.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"
                )
            );

            return await _httpClient.SendAsync(
                solicitud,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }
    }
}