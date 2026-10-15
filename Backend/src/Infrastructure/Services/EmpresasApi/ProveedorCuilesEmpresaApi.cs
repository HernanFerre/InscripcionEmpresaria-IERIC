using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    public sealed class ProveedorCuilesEmpresaApi
        : IProveedorCuilesEmpresa,
          IProveedorNominaEmpresa
    {
        private const int LimiteTrabajadoresNomina = 100;

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
            List<CuilEmpresaApiResponse> resultados =
                await ObtenerResultadosAsync(
                    cuitEmpresa,
                    limite,
                    cancellationToken
                );

            return resultados
                .Select(resultado => resultado.Cuil)
                .Distinct()
                .Select(numero => new Cuil(numero))
                .ToList();
        }

        public async Task<IReadOnlyCollection<TrabajadorNomina>>
            ObtenerNominaPorCuitAsync(
                Cuit cuitEmpresa,
                long solicitudId,
                CancellationToken cancellationToken = default
            )
        {
            if (solicitudId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(solicitudId),
                    "La solicitud de la nómina no es válida."
                );
            }

            List<CuilEmpresaApiResponse> resultados =
                await ObtenerResultadosAsync(
                    cuitEmpresa,
                    LimiteTrabajadoresNomina,
                    cancellationToken
                );

            return resultados
                .Select(resultado => CrearTrabajador(
                    resultado,
                    solicitudId
                ))
                .OrderByDescending(trabajador => trabajador.Ingreso)
                .ThenBy(
                    trabajador => trabajador.ApellidoYNombre,
                    StringComparer.OrdinalIgnoreCase
                )
                .ThenBy(trabajador => trabajador.Cuil.ToInt64())
                .ToList();
        }

        private async Task<List<CuilEmpresaApiResponse>>
            ObtenerResultadosAsync(
                Cuit cuitEmpresa,
                int limite,
                CancellationToken cancellationToken
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

                if (resultados.Any(resultado => resultado == null))
                {
                    throw new HttpRequestException(
                        "El servicio de empresas devolvió registros vacíos."
                    );
                }

                bool contieneCuilInvalido = resultados.Any(
                    resultado => !Cuil.EsCuilValido(resultado.Cuil)
                );

                if (contieneCuilInvalido)
                {
                    throw new HttpRequestException(
                        "El servicio de empresas devolvió uno o más CUILes inválidos."
                    );
                }

                return resultados;
            }
        }

        private static TrabajadorNomina CrearTrabajador(
            CuilEmpresaApiResponse resultado,
            long solicitudId
        )
        {
            string apellidoYNombre =
                $"{resultado.Apellidos?.Trim()} {resultado.Nombres?.Trim()}"
                    .Trim();

            DateTime? egreso = resultado.FechaEgreso?.Date;

            if (egreso == new DateTime(9998, 12, 31))
            {
                egreso = null;
            }

            try
            {
                return TrabajadorNomina.CrearDesdeServicio(
                    solicitudId,
                    apellidoYNombre,
                    new Cuil(resultado.Cuil),
                    resultado.FechaIngreso,
                    egreso
                );
            }
            catch (SumariosDomainException exception)
            {
                throw new HttpRequestException(
                    $"El servicio de empresas devolvió datos inválidos " +
                    $"para el trabajador con CUIL {resultado.Cuil}: " +
                    exception.Message,
                    exception
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