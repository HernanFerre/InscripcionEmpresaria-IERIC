using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    public sealed class ProveedorCuilesEmpresaApi
        : IProveedorCuilesEmpresa
    {
        private readonly HttpClient _httpClient;

        public ProveedorCuilesEmpresaApi(
            HttpClient httpClient
        )
        {
            _httpClient =
                httpClient ??
                throw new ArgumentNullException(
                    nameof(httpClient)
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

            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    url,
                    cancellationToken
                );

            response.EnsureSuccessStatusCode();

            List<CuilEmpresaApiResponse> resultados =
                await response.Content.ReadFromJsonAsync<
                    List<CuilEmpresaApiResponse>
                >(
                    cancellationToken: cancellationToken
                );

            if (
                resultados == null ||
                resultados.Count == 0
            )
            {
                return Array.Empty<Cuil>();
            }

            List<long> numerosCuil =
                resultados
                    .Select(resultado => resultado.Cuil)
                    .Distinct()
                    .ToList();

            bool contieneCuilInvalido =
                numerosCuil.Any(
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
}