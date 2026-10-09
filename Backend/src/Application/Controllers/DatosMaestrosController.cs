using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IERIC.SumariosIERIC.Application
{
    [AllowAnonymous]
    [Route("v1/datos-maestros")]
    [ApiController]
    public class DatosMaestrosController : ControllerBase
    {
        private readonly IProveedorDatosMaestros _proveedorDatosMaestros;

        public DatosMaestrosController(
            IProveedorDatosMaestros proveedorDatosMaestros
        )
        {
            _proveedorDatosMaestros = proveedorDatosMaestros;
        }

        [HttpGet("actividades-construccion")]
        public async Task<IActionResult> ObtenerActividadesConstruccionAsync(
            CancellationToken cancellationToken
        )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerActividadesConstruccionAsync(cancellationToken);

            return Content(contenido, "application/json");
        }

        [HttpGet("caracteres-empresa")]
        public async Task<IActionResult> ObtenerCaracteresEmpresaAsync(
            CancellationToken cancellationToken
        )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerCaracteresEmpresaAsync(cancellationToken);

            return Content(contenido, "application/json");
        }

        [HttpGet("tipos-sociedad")]
        public async Task<IActionResult> ObtenerTiposSociedadAsync(
            CancellationToken cancellationToken
        )
        {
            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerTiposSociedadAsync(cancellationToken);

            return Content(contenido, "application/json");
        }
        [HttpGet("tipos-representantes")]
        public async Task<IActionResult>
            ObtenerTiposRepresentantesAsync(
                [FromQuery] int tipoSociedad,
                CancellationToken cancellationToken
            )
        {
            if (tipoSociedad <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe indicar un tipo de sociedad válido."
                    }
                );
            }

            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerTiposRepresentantesAsync(
                        tipoSociedad,
                        cancellationToken
                    );

            return Content(contenido, "application/json");
        }

        [HttpGet("localidades")]
        public async Task<IActionResult> ObtenerLocalidadesAsync(
            [FromQuery] string codigoPostal,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                return BadRequest(
                    new
                    {
                        mensaje = "Debe indicar el código postal."
                    }
                );
            }

            string contenido =
                await _proveedorDatosMaestros
                    .ObtenerLocalidadesAsync(
                        codigoPostal,
                        cancellationToken
                    );

            return Content(contenido, "application/json");
        }
    }
}