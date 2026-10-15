using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    public sealed class ProveedorCuilesEmpresaApi
        : IProveedorCuilesEmpresa
    {
        private static readonly JsonSerializerOptions OpcionesJson =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private readonly HttpClient _httpClient;

        private readonly IProveedorTokenDatosMaestros _proveedorToken;

        public ProveedorCuilesEmpresaApi(
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

        public async Task<IReadOnlyCollection<Cuil>>
            ObtenerPorCuitAsync(
                Cuit cuitEmpresa,
                int limite,
                CancellationToken cancellationToken = default
            )
        {
            if (cuitEmpresa == null)
            {
                throw new ArgumentNullException(
                    nameof(cuitEmpresa)
                );
            }

            if (limite <= 0 || limite > 1000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limite),
                    "El límite debe ser mayor que cero y menor o igual a 1000."
                );
            }

            string url =
                $"empresas/consulta-cuiles/" +
                $"{cuitEmpresa}?limit={limite}";

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

                string contenido = await response.Content.ReadAsStringAsync(
                    cancellationToken
                );

                List<CuilEmpresaApiResponse> resultados =
                    LeerResultados(contenido);

                if (resultados.Count == 0)
                {
                    return Array.Empty<Cuil>();
                }

                if (resultados.Any(resultado => resultado == null))
                {
                    throw new HttpRequestException(
                        "El servicio de empresas devolvió registros vacíos."
                    );
                }

                List<long> numerosCuil = resultados
                    .Select(resultado => resultado.Cuil)
                    .Distinct()
                    .ToList();

                bool contieneCuilInvalido = numerosCuil.Any(
                    numero => !Cuil.EsCuilValido(numero)
                );

                if (contieneCuilInvalido)
                {
                    throw new HttpRequestException(
                        "El servicio de empresas devolvió uno o más CUILes inválidos."
                    );
                }

                return numerosCuil
                    .Select(numero => new Cuil(numero))
                    .ToList();
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

        private static List<CuilEmpresaApiResponse> LeerResultados(
            string contenido
        )
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                throw new HttpRequestException(
                    "El servicio de empresas devolvió una respuesta vacía."
                );
            }

            try
            {
                using JsonDocument documento = JsonDocument.Parse(contenido);
                JsonElement listado = documento.RootElement;

                if (listado.ValueKind == JsonValueKind.Object &&
                    listado.TryGetProperty("value", out JsonElement valor))
                {
                    listado = valor;
                }

                if (listado.ValueKind != JsonValueKind.Array)
                {
                    throw new HttpRequestException(
                        "El servicio de empresas no devolvió una lista de trabajadores."
                    );
                }

                return listado.Deserialize<List<CuilEmpresaApiResponse>>(
                    OpcionesJson
                ) ?? new List<CuilEmpresaApiResponse>();
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException(
                    "El servicio de empresas devolvió una respuesta JSON inválida.",
                    exception
                );
            }
        }
    }
}