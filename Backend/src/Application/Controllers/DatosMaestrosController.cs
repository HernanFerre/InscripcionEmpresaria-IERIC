using System;
using System.Threading;
using System.Threading.Tasks;

using IERIC.SumariosIERIC.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IERIC.SumariosIERIC.Application
{
    // Temporal durante la etapa de desarrollo.
    // El backend se autentica contra Datos Maestros.
    [AllowAnonymous]
    [Route("v1/datos-maestros")]
    [ApiController]
    public class DatosMaestrosController : ControllerBase
    {
        private readonly IProveedorDatosMaestros
            _proveedorDatosMaestros;

        public DatosMaestrosController(
            IProveedorDatosMaestros proveedorDatosMaestros
        )
        {
            _proveedorDatosMaestros =
                proveedorDatosMaestros ??
                throw new ArgumentNullException(
                    nameof(proveedorDatosMaestros)
                );
        }

        [HttpGet("actividades-construccion")]
        public async Task<IActionResult>
            ObtenerActividadesConstruccionAsync(
                CancellationToken cancellationToken
            )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerActividadesConstruccionAsync(
                        cancellationToken
                    );

            return Content(
                contenido,
                "application/json"
            );
        }

        [HttpGet("caracteres-empresa")]
        public async Task<IActionResult>
            ObtenerCaracteresEmpresaAsync(
                CancellationToken cancellationToken
            )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerCaracteresEmpresaAsync(
                        cancellationToken
                    );

            return Content(
                contenido,
                "application/json"
            );
        }

        [HttpGet("tipos-sociedad")]
        public async Task<IActionResult>
            ObtenerTiposSociedadAsync(
                CancellationToken cancellationToken
            )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerTiposSociedadAsync(
                        cancellationToken
                    );

            return Content(
                contenido,
                "application/json"
            );
        }
    }
}