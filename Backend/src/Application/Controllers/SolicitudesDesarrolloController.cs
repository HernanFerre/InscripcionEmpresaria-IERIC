using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Commands;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using IERIC.SumariosIERIC.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace IERIC.SumariosIERIC.Application
{
    [AllowAnonymous]
    [Route("v1/desarrollo/solicitudes")]
    [ApiController]
    public class SolicitudesDesarrolloController
        : ControllerBase
    {
        private const string BearerPrefix = "Bearer ";

        private readonly IMediator _mediator;
        private readonly IProveedorTokenDatosMaestros _proveedorToken;
        private readonly IWebHostEnvironment _environment;

        public SolicitudesDesarrolloController(
            IMediator mediator,
            IProveedorTokenDatosMaestros proveedorToken,
            IWebHostEnvironment environment
        )
        {
            _mediator =
                mediator ??
                throw new ArgumentNullException(nameof(mediator));

            _proveedorToken =
                proveedorToken ??
                throw new ArgumentNullException(nameof(proveedorToken));

            _environment =
                environment ??
                throw new ArgumentNullException(nameof(environment));
        }

        [HttpPost("empresa")]
        public async Task<
            ActionResult<GuardarEmpresaSolicitudResponse>
        > GuardarEmpresaAsync(
            [FromBody]
            GuardarEmpresaSolicitudRequest request,
            CancellationToken cancellationToken
        )
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            if (request == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La solicitud no puede estar vacía."
                    }
                );
            }

            string tokenRecibido = ObtenerBearerToken();

            if (string.IsNullOrWhiteSpace(tokenRecibido))
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "No se recibió el token de desarrollo."
                    }
                );
            }

            string tokenEsperado =
                await _proveedorToken.ObtenerTokenAsync(
                    cancellationToken
                );

            if (!TokensCoinciden(tokenRecibido, tokenEsperado))
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "El token de desarrollo no es válido."
                    }
                );
            }

            string usuarioId =
                ObtenerUsuarioId(tokenRecibido);

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "No fue posible identificar " +
                            "al usuario autenticado."
                    }
                );
            }

            GuardarEmpresaSolicitudCommand command =
                new GuardarEmpresaSolicitudCommand(
                    request,
                    usuarioId
                );

            GuardarEmpresaSolicitudResponse resultado =
                await _mediator.Send(
                    command,
                    cancellationToken
                );

            return Ok(resultado);
        }

        private string ObtenerBearerToken()
        {
            if (
                !Request.Headers.TryGetValue(
                    "Authorization",
                    out StringValues authorization
                )
            )
            {
                return null;
            }

            string valor = authorization.FirstOrDefault();

            if (
                string.IsNullOrWhiteSpace(valor) ||
                !valor.StartsWith(
                    BearerPrefix,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return null;
            }

            return valor.Substring(BearerPrefix.Length).Trim();
        }

        private static bool TokensCoinciden(
            string tokenRecibido,
            string tokenEsperado
        )
        {
            byte[] recibido =
                Encoding.UTF8.GetBytes(tokenRecibido);

            byte[] esperado =
                Encoding.UTF8.GetBytes(tokenEsperado);

            return
                recibido.Length == esperado.Length &&
                CryptographicOperations.FixedTimeEquals(
                    recibido,
                    esperado
                );
        }

        private static string ObtenerUsuarioId(
            string token
        )
        {
            try
            {
                JwtSecurityToken jwt =
                    new JwtSecurityTokenHandler()
                        .ReadJwtToken(token);

                string[] tiposIdentificador =
                {
                    ClaimTypes.NameIdentifier,
                    JwtRegisteredClaimNames.Sub,
                    "nameid",
                    "id",
                    "usuarioId",
                    "userId"
                };

                foreach (string tipo in tiposIdentificador)
                {
                    string valor =
                        jwt.Claims.FirstOrDefault(
                            claim => claim.Type == tipo
                        )?.Value;

                    if (
                        Guid.TryParse(
                            valor?.Trim(),
                            out Guid usuarioId
                        )
                    )
                    {
                        return usuarioId.ToString();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
