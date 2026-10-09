using System;
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
    public class GuardarEmpresaSolicitudCommandHandler
        : IRequestHandler<
            GuardarEmpresaSolicitudCommand,
            GuardarEmpresaSolicitudResponse
        >
    {
        private readonly ISolicitudInscripcionRepository
            _solicitudInscripcionRepository;

        public GuardarEmpresaSolicitudCommandHandler(
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

        public async Task<GuardarEmpresaSolicitudResponse> Handle(
            GuardarEmpresaSolicitudCommand command,
            CancellationToken cancellationToken
        )
        {
            if (command == null)
            {
                throw new ArgumentNullException(
                    nameof(command)
                );
            }

            GuardarEmpresaSolicitudValidator.Validar(
                command.Request
            );

            if (
                !Guid.TryParse(
                    command.UsuarioId?.Trim(),
                    out Guid usuarioId
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
                long cuitPrincipal =
                    GuardarEmpresaSolicitudValidator
                        .ObtenerCuitNormalizado(
                            command.Request.Cuit,
                            "empresa principal"
                        );

                Empresa empresaPrincipal =
                    Empresa.CrearPrincipal(
                        new Cuit(cuitPrincipal),
                        command.Request.RazonSocial,
                        command.Request.TipoSociedadId,
                        command.Request.ActividadId,
                        command.Request.CaracterId,
                        command.Request.Calle,
                        command.Request.Numero,
                        command.Request.Piso,
                        command.Request.DepartamentoOficina,
                        command.Request.CodigoPostal,
                        command.Request.Provincia,
                        command.Request.Localidad,
                        command.Request.Correo,
                        command.Request.Telefono
                    );

                SolicitudInscripcion solicitud =
                    SolicitudInscripcion.Crear(
                        usuarioId,
                        empresaPrincipal
                    );

                foreach (
                    EmpresaIntegranteRequest integranteRequest
                    in command.Request.EmpresasIntegrantes
                )
                {
                    long cuitIntegrante =
                        GuardarEmpresaSolicitudValidator
                            .ObtenerCuitNormalizado(
                                integranteRequest.Cuit,
                                "empresa integrante"
                            );

                    Empresa empresaIntegrante =
                        Empresa.CrearIntegrante(
                            new Cuit(cuitIntegrante),
                            integranteRequest.RazonSocial,
                            integranteRequest.TipoSociedadId
                        );

                    solicitud.AgregarEmpresaIntegrante(
                        empresaIntegrante
                    );
                }

                await _solicitudInscripcionRepository
                    .GuardarAsync(
                        solicitud
                    );

                GuardarEmpresaSolicitudResponse response =
                    new GuardarEmpresaSolicitudResponse
                    {
                        SolicitudId =
                            solicitud.Id,

                        EmpresaId =
                            solicitud
                                .EmpresaPrincipal
                                .Id,

                        CuitEmpresa =
                            solicitud
                                .EmpresaPrincipal
                                .Cuit
                                .ToInt64()
                                .ToString(),

                        CantidadEmpresasIntegrantes =
                            solicitud
                                .EmpresasIntegrantes
                                .Count
                    };

                int legacyIdIntegrante = 2;

                foreach (
                    EmpresaIntegrante relacion
                    in solicitud.EmpresasIntegrantes
                )
                {
                    response.EmpresasIntegrantes.Add(
                        new EmpresaIntegranteGuardadaResponse
                        {
                            EmpresaId =
                                relacion
                                    .Integrante
                                    .Id,

                            Cuit =
                                relacion
                                    .Integrante
                                    .Cuit
                                    .ToInt64()
                                    .ToString(),

                            RazonSocial =
                                relacion
                                    .Integrante
                                    .RazonSocial,

                            TipoSociedadId =
                                relacion
                                    .Integrante
                                    .TipoSociedadId,

                            LegacyId =
                                legacyIdIntegrante
                        }
                    );

                    legacyIdIntegrante++;
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