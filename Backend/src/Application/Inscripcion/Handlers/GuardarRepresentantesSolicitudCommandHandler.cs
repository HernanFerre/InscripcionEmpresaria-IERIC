using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using IERIC.SumariosIERIC.Application.Inscripcion.Validation;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class GuardarRepresentantesSolicitudCommandHandler
        : IRequestHandler<
            GuardarRepresentantesSolicitudCommand,
            GuardarRepresentantesSolicitudResponse
        >
    {
        private readonly ISolicitudInscripcionRepository
            _solicitudInscripcionRepository;

        public GuardarRepresentantesSolicitudCommandHandler(
            ISolicitudInscripcionRepository
                solicitudInscripcionRepository
        )
        {
            _solicitudInscripcionRepository =
                solicitudInscripcionRepository ??
                throw new ArgumentNullException(
                    nameof(solicitudInscripcionRepository)
                );
        }

        public async Task<GuardarRepresentantesSolicitudResponse>
            Handle(
                GuardarRepresentantesSolicitudCommand command,
                CancellationToken cancellationToken
            )
        {
            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command)
                );
            }

            GuardarRepresentantesSolicitudValidator.Validar(
                command.SolicitudId,
                command.Request
            );

            if (
                !Guid.TryParse(
                    command.IdUsuarioAlta?.Trim(),
                    out Guid idUsuarioAlta
                )
            )
            {
                throw new InvalidException(
                    "No fue posible identificar al usuario autenticado.",
                    "Vuelva a iniciar sesión e intente nuevamente."
                );
            }

            try
            {
                List<Representante> representantes =
                    new List<Representante>();

                List<RepresentanteSolicitudRequest>
                    representantesRequest =
                        command.Request.Representantes ??
                        new List<RepresentanteSolicitudRequest>();

                foreach (
                    RepresentanteSolicitudRequest representanteRequest
                    in representantesRequest
                )
                {
                    long cuil =
                        GuardarRepresentantesSolicitudValidator
                            .ObtenerCuilNormalizado(
                                representanteRequest.Cuil
                            );

                    representantes.Add(
                        Representante.Crear(
                            representanteRequest.EmpresaId,
                            representanteRequest.Apellido,
                            representanteRequest.Nombre,
                            new Cuil(cuil),
                            representanteRequest.TipoRepresentanteId,
                            representanteRequest.Correo,
                            representanteRequest.Telefono
                        )
                    );
                }

                await _solicitudInscripcionRepository
                    .GuardarRepresentantesAsync(
                        command.SolicitudId,
                        idUsuarioAlta,
                        representantes,
                        cancellationToken
                    );

                GuardarRepresentantesSolicitudResponse response =
                    new GuardarRepresentantesSolicitudResponse
                    {
                        SolicitudId = command.SolicitudId,
                        CantidadRepresentantes =
                            representantes.Count
                    };

                foreach (
                    Representante representante
                    in representantes
                )
                {
                    response.Representantes.Add(
                        new RepresentanteGuardadoResponse
                        {
                            RepresentanteId =
                                representante.Id,

                            EmpresaId =
                                representante.EmpresaId,

                            Apellido =
                                representante.Apellido,

                            Nombre =
                                representante.Nombre,

                            Cuil =
                                representante.Cuil.ToString(),

                            TipoRepresentanteId =
                                representante.TipoRepresentanteId,

                            Correo =
                                representante.Correo,

                            Telefono =
                                representante.Telefono
                        }
                    );
                }

                return response;
            }
            catch (InvalidException)
            {
                throw;
            }
            catch (SumariosDomainException exception)
            {
                throw new InvalidException(
                    exception.Message,
                    "Revise los datos ingresados.",
                    exception
                );
            }
        }
    }
}