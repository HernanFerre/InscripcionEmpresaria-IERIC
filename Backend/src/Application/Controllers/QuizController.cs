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

        [Route("Crear/{cuit}")]
        [HttpGet]
        public async Task<IActionResult> CrearQuizAsync( double cuit)
        {

            //Consulta para traer los datos y construir el quiz para exponer.

            
            return Ok("este es el quiz");
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