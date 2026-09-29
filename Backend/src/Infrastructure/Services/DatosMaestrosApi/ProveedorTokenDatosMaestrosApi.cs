using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;

namespace IERIC.SumariosIERIC.Infrastructure.Services.DatosMaestrosApi
{
    public sealed class ProveedorTokenDatosMaestrosApi
        : IProveedorTokenDatosMaestros
    {
        private readonly HttpClient _httpClient;

        private readonly DatosMaestrosApiConfiguracion
            _configuracion;

        private readonly EstadoTokenDatosMaestros
            _estadoToken;

        public ProveedorTokenDatosMaestrosApi(
            HttpClient httpClient,
            DatosMaestrosApiConfiguracion configuracion,
            EstadoTokenDatosMaestros estadoToken
        )
        {
            _httpClient = httpClient;
            _configuracion = configuracion;
            _estadoToken = estadoToken;
        }

        public async Task<string> ObtenerTokenAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (
                _estadoToken.IntentarObtenerTokenVigente(
                    out string tokenVigente
                )
            )
            {
                return tokenVigente;
            }

            await _estadoToken.Renovacion.WaitAsync(
                cancellationToken
            );

            try
            {
                if (
                    _estadoToken.IntentarObtenerTokenVigente(
                        out tokenVigente
                    )
                )
                {
                    return tokenVigente;
                }

                ValidarConfiguracion();

                var cuerpoSolicitud = new
                {
                    usuario = _configuracion.Usuario,
                    password = _configuracion.Password,
                    publica = _configuracion.Publica
                };

                using HttpResponseMessage respuesta =
                    await _httpClient.PostAsJsonAsync(
                        "auth/v1/authenticate",
                        cuerpoSolicitud,
                        cancellationToken
                    );

                string contenido =
                    await respuesta.Content.ReadAsStringAsync(
                        cancellationToken
                    );

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        "No fue posible obtener el token de Datos Maestros. " +
                        $"Código HTTP: {(int)respuesta.StatusCode}."
                    );
                }

                string token = NormalizarToken(contenido);

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException(
                        "La autenticación de Datos Maestros devolvió un token vacío."
                    );
                }

                int minutosVigencia =
                    Math.Max(
                        1,
                        _configuracion.MinutosDuracionToken - 5
                    );

                _estadoToken.Guardar(
                    token,
                    DateTimeOffset.UtcNow.AddMinutes(
                        minutosVigencia
                    )
                );

                return token;
            }
            finally
            {
                _estadoToken.Renovacion.Release();
            }
        }

        public void InvalidarToken()
        {
            _estadoToken.Invalidar();
        }

        private void ValidarConfiguracion()
        {
            if (
                string.IsNullOrWhiteSpace(
                    _configuracion.Usuario
                )
            )
            {
                throw new InvalidOperationException(
                    "No se configuró DatosMaestrosApi:Usuario."
                );
            }

            if (
                string.IsNullOrWhiteSpace(
                    _configuracion.Password
                )
            )
            {
                throw new InvalidOperationException(
                    "No se configuró DatosMaestrosApi:Password."
                );
            }

            if (
                _configuracion.MinutosDuracionToken <= 0
            )
            {
                throw new InvalidOperationException(
                    "DatosMaestrosApi:MinutosDuracionToken no es válido."
                );
            }
        }

        private static string NormalizarToken(
            string contenido
        )
        {
            string valor = contenido?.Trim() ?? string.Empty;

            if (
                valor.StartsWith("\"") &&
                valor.EndsWith("\"")
            )
            {
                string tokenDeserializado =
                    JsonSerializer.Deserialize<string>(
                        valor
                    );

                return tokenDeserializado?.Trim() ??
                    string.Empty;
            }

            return valor;
        }
    }
}