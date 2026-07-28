using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IERIC.SumariosIERIC.Application.Commands;
using IERIC.SumariosIERIC.Application.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Quiz.Models;

namespace IERIC.SumariosIERIC.Application
{
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
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        [Route("Crear")]
        [HttpPost]
        public async Task<ActionResult<QuizResponse>> CrearQuizAsync(
            [FromBody] CrearQuizRequest request
        )
        {
            if (request == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje = "La solicitud no puede estar vacía."
                    }
                );
            }

            if (string.IsNullOrWhiteSpace(request.Cuit))
            {
                return BadRequest(
                    new
                    {
                        mensaje = "Debe informar el CUIT de la empresa."
                    }
                );
            }

            if (request.Cuiles == null || request.Cuiles.Count == 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje = "Debe informar al menos un CUIL."
                    }
                );
            }

            CrearQuizCommand command =
                new CrearQuizCommand(
                    request.Cuit,
                    request.Cuiles
                );

            QuizResponse quiz =
                await _mediator.Send(command);

            return Ok(quiz);
        }

        [Route("Validar/{cuit}/{cuil}")]
        [HttpGet]
        public async Task<ActionResult> ValidarAsync(double cuit, double cuil)
        {

            // recibo la respuesta del quiz y la valido con la info de la emrpesa almacenada.

            return Ok("este es el quiz");

        }


    }
}