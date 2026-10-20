using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace IERIC.SumariosIERIC.Application
{
    [Route("v1/empresas")]
    [ApiController]
    public class EmpresasController : ControllerBase
    {
        private readonly IProveedorEstadoEmpresa _proveedorEstadoEmpresa;
        private readonly IProveedorTokenDatosMaestros _proveedorToken;
        private readonly IWebHostEnvironment _environment;

        public EmpresasController(
            IProveedorEstadoEmpresa proveedorEstadoEmpresa,
            IProveedorTokenDatosMaestros proveedorToken,
            IWebHostEnvironment environment
        )
        {
            _proveedorEstadoEmpresa = proveedorEstadoEmpresa
                ?? throw new ArgumentNullException(nameof(proveedorEstadoEmpresa));

            _proveedorToken = proveedorToken
                ?? throw new ArgumentNullException(nameof(proveedorToken));

            _environment = environment
                ?? throw new ArgumentNullException(nameof(environment));
        }

        [Authorize(
            AuthenticationSchemes = global::Auth.CustomExtensionsMethods
                .PublicAuthenticationScheme
        )]
        [HttpGet("estado/{cuit}")]
        public Task<IActionResult> ObtenerEstadoAsync(
            string cuit,
            CancellationToken cancellationToken
        )
        {
            return ConsultarEstadoAsync(cuit, cancellationToken);
        }

        [AllowAnonymous]
        [HttpGet("/v1/desarrollo/empresas/estado/{cuit}")]
        public async Task<IActionResult> ObtenerEstadoDesarrolloAsync(
            string cuit,
            CancellationToken cancellationToken
        )
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            if (!AuthenticationHeaderValue.TryParse(
                    Request.Headers["Authorization"].ToString(),
                    out AuthenticationHeaderValue authorization
                ) ||
                !string.Equals(
                    authorization.Scheme,
                    "Bearer",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.IsNullOrWhiteSpace(authorization.Parameter))
            {
                return Unauthorized(new
                {
                    mensaje = "No se recibió el token de desarrollo."
                });
            }

            string tokenEsperado = await _proveedorToken.ObtenerTokenAsync(
                cancellationToken
            );

            byte[] recibido = Encoding.UTF8.GetBytes(
                authorization.Parameter.Trim()
            );

            byte[] esperado = Encoding.UTF8.GetBytes(tokenEsperado);

            if (!CryptographicOperations.FixedTimeEquals(recibido, esperado))
            {
                return Unauthorized(new
                {
                    mensaje = "El token de desarrollo no es válido."
                });
            }

            return await ConsultarEstadoAsync(cuit, cancellationToken);
        }

        private async Task<IActionResult> ConsultarEstadoAsync(
            string cuit,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(cuit) ||
                cuit.Length != 11 ||
                !long.TryParse(
                    cuit,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out long numeroCuit
                ) ||
                !Cuit.EsCuitValido(numeroCuit))
            {
                return BadRequest(new
                {
                    mensaje = "El CUIT debe contener 11 números y un dígito verificador válido."
                });
            }

            Response.Headers["Cache-Control"] = "no-store";

            try
            {
                EstadoInscripcionEmpresa estado =
                    await _proveedorEstadoEmpresa.ObtenerPorCuitAsync(
                        new Cuit(numeroCuit),
                        cancellationToken
                    );

                return Ok(new
                {
                    cuit = estado.CuitEmpresa.ToInt64().ToString(),
                    razon = estado.RazonSocial,
                    codigoEstado = estado.CodigoEstado,
                    mensaje = estado.Mensaje
                });
            }
            catch (HttpRequestException exception)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    mensaje = "No fue posible consultar el estado de la empresa. "
                        + exception.Message
                });
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    mensaje = "El servicio de empresas devolvió una respuesta inválida."
                });
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(StatusCodes.Status504GatewayTimeout, new
                {
                    mensaje = "El servicio de empresas no respondió a tiempo. Intente nuevamente."
                });
            }
        }
    }
}