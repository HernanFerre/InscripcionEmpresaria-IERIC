using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Commands;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IERIC.SumariosIERIC.Application
{
    [Authorize]
    [Route("v1/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<QuizController> _logger;

        public QuizController(
            IMediator mediator,
            ILogger<QuizController> logger
        )
        {
            _mediator =
                mediator ??
                throw new ArgumentNullException(
                    nameof(mediator)
                );

            _logger =
                logger ??
                throw new ArgumentNullException(
                    nameof(logger)
                );
        }

        [Route("Crear")]
        [HttpPost]
        public async Task<ActionResult<QuizResponse>>
            CrearQuizAsync(
                [FromBody] CrearQuizRequest request
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

            if (string.IsNullOrWhiteSpace(request.Cuit))
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe informar el CUIT de la empresa."
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

            CrearQuizCommand command =
                new CrearQuizCommand(
                    request.Cuit,
                    usuarioId
                );

            QuizResponse quiz =
                await _mediator.Send(
                    command
                );

            return Ok(
                quiz
            );
        }

        [Route("Validar")]
        [HttpPost]
        public async Task<ActionResult<ValidarQuizResponse>>
            ValidarAsync(
                [FromBody] ValidarQuizRequest request
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

            if (request.QuizId <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe informar el identificador del quiz."
                    }
                );
            }

            if (
                request.OpcionesSeleccionadas == null ||
                request.OpcionesSeleccionadas.Count == 0
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe seleccionar al menos una opción."
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

            ValidarQuizCommand command =
                new ValidarQuizCommand(
                    request.QuizId,
                    usuarioId,
                    request.OpcionesSeleccionadas
                );

            ValidarQuizResponse resultado =
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