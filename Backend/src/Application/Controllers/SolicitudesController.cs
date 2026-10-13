using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Commands;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IERIC.SumariosIERIC.Application
{
    [Authorize(
        AuthenticationSchemes =
            global::Auth.CustomExtensionsMethods
                .PublicAuthenticationScheme
    )]
    [Route("v1/solicitudes")]
    [ApiController]
    public class SolicitudesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SolicitudesController(
            IMediator mediator
        )
        {
            _mediator =
                mediator ??
                throw new ArgumentNullException(
                    nameof(mediator)
                );
        }

        [HttpPost("empresa")]
        public async Task<
            ActionResult<GuardarEmpresaSolicitudResponse>
        > GuardarEmpresaAsync(
            [FromBody]
            GuardarEmpresaSolicitudRequest request
        )
        {
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

            string idUsuarioAlta =
                ObtenerIdUsuarioAlta();

            if (string.IsNullOrWhiteSpace(idUsuarioAlta))
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
                    idUsuarioAlta
                );

            GuardarEmpresaSolicitudResponse resultado =
                await _mediator.Send(
                    command
                );

            return Ok(
                resultado
            );
        }

        [HttpPut("{solicitudId:long}/representantes")]
        public async Task<
            ActionResult<GuardarRepresentantesSolicitudResponse>
        > GuardarRepresentantesAsync(
            long solicitudId,
            [FromBody]
            GuardarRepresentantesSolicitudRequest request,
            CancellationToken cancellationToken
        )
        {
            if (request == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La solicitud de representantes " +
                            "no puede estar vacía."
                    }
                );
            }

            string idUsuarioAlta =
                ObtenerIdUsuarioAlta();

            if (string.IsNullOrWhiteSpace(idUsuarioAlta))
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

            GuardarRepresentantesSolicitudCommand command =
                new GuardarRepresentantesSolicitudCommand(
                    solicitudId,
                    request,
                    idUsuarioAlta
                );

            GuardarRepresentantesSolicitudResponse resultado =
                await _mediator.Send(
                    command,
                    cancellationToken
                );

            return Ok(resultado);
        }

        private string ObtenerIdUsuarioAlta()
        {
            return
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                )
                ??
                User.FindFirstValue(
                    "sub"
                );
        }
    }
}