using System;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace IERIC.SumariosIERIC.Application
{
    [AllowAnonymous]
    [Route("v1/autenticacion-desarrollo")]
    [ApiController]
    public class AutenticacionDesarrolloController
        : ControllerBase
    {
        private readonly IProveedorTokenDatosMaestros
            _proveedorToken;

        private readonly IWebHostEnvironment
            _environment;

        public AutenticacionDesarrolloController(
            IProveedorTokenDatosMaestros proveedorToken,
            IWebHostEnvironment environment
        )
        {
            _proveedorToken =
                proveedorToken ??
                throw new ArgumentNullException(
                    nameof(proveedorToken)
                );

            _environment =
                environment ??
                throw new ArgumentNullException(
                    nameof(environment)
                );
        }

        [HttpGet("token")]
        public async Task<IActionResult>
            ObtenerTokenAsync(
                CancellationToken cancellationToken
            )
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            string token =
                await _proveedorToken
                    .ObtenerTokenAsync(
                        cancellationToken
                    );

            Response.Headers["Cache-Control"] =
                "no-store";

            return Ok(
                new
                {
                    token
                }
            );
        }
    }
}