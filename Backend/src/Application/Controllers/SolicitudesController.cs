using System;
using System.Security.Claims;
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

            string usuarioId =
                ObtenerUsuarioId();

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
                    command
                );

            return Ok(
                resultado
            );
        }

        private string ObtenerUsuarioId()
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