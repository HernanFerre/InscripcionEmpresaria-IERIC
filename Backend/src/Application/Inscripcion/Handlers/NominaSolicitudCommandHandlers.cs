using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class NominaSolicitudCommandHandlers
        : IRequestHandler<CargarNominaSolicitudCommand, NominaSolicitudResponse>,
          IRequestHandler<GuardarNominaSolicitudCommand, NominaSolicitudResponse>
    {
        private readonly INominaSolicitudRepository _nominaRepository;
        private readonly IProveedorNominaEmpresa _proveedorNomina;

        public NominaSolicitudCommandHandlers(
            INominaSolicitudRepository nominaRepository,
            IProveedorNominaEmpresa proveedorNomina
        )
        {
            _nominaRepository = nominaRepository ??
                throw new ArgumentNullException(nameof(nominaRepository));

            _proveedorNomina = proveedorNomina ??
                throw new ArgumentNullException(nameof(proveedorNomina));
        }

        public async Task<NominaSolicitudResponse> Handle(
            CargarNominaSolicitudCommand command,
            CancellationToken cancellationToken
        )
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Guid idUsuarioAlta = ValidarSolicitudYUsuario(
                command.SolicitudId,
                command.IdUsuarioAlta
            );

            try
            {
                IReadOnlyCollection<TrabajadorNomina> trabajadores =
                    await CargarTrabajadoresAsync(
                        command.SolicitudId,
                        idUsuarioAlta,
                        cancellationToken
                    );

                return CrearRespuesta(command.SolicitudId, trabajadores);
            }
            catch (SumariosDomainException exception)
            {
                throw new InvalidException(
                    exception.Message,
                    "Revise la solicitud e intente nuevamente.",
                    exception
                );
            }
        }

        public async Task<NominaSolicitudResponse> Handle(
            GuardarNominaSolicitudCommand command,
            CancellationToken cancellationToken
        )
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Guid idUsuarioAlta = ValidarSolicitudYUsuario(
                command.SolicitudId,
                command.IdUsuarioAlta
            );

            if (command.Request?.Trabajadores == null)
            {
                throw new InvalidException(
                    "No se informó una nómina válida.",
                    "Revise los trabajadores ingresados."
                );
            }

            try
            {
                IReadOnlyCollection<TrabajadorNomina> actuales =
                    await CargarTrabajadoresAsync(
                        command.SolicitudId,
                        idUsuarioAlta,
                        cancellationToken
                    );

                Dictionary<long, TrabajadorNomina> porId = actuales
                    .ToDictionary(trabajador => trabajador.Id);

                List<TrabajadorNomina> trabajadores =
                    new List<TrabajadorNomina>();

                for (int indice = 0;
                    indice < command.Request.Trabajadores.Count;
                    indice++)
                {
                    TrabajadorNominaSolicitudRequest request =
                        command.Request.Trabajadores[indice];

                    if (request == null)
                    {
                        throw new InvalidException(
                            $"La fila {indice + 1} de la nómina está vacía.",
                            "Revise los trabajadores ingresados."
                        );
                    }

                    try
                    {
                        trabajadores.Add(CrearTrabajadorSolicitado(
                            command.SolicitudId,
                            request,
                            porId
                        ));
                    }
                    catch (SumariosDomainException exception)
                    {
                        string nombre = string.IsNullOrWhiteSpace(request.ApellidoYNombre)
                            ? $"Trabajador de la fila {indice + 1}"
                            : request.ApellidoYNombre.Trim();

                        string cuil = string.IsNullOrWhiteSpace(request.Cuil)
                            ? "sin informar"
                            : request.Cuil.Trim();

                        throw new InvalidException(
                            $"{nombre} (CUIL {cuil}): {exception.Message}",
                            "Revise los datos de ese trabajador.",
                            exception
                        );
                    }
                }

                IReadOnlyCollection<TrabajadorNomina> guardados =
                    await _nominaRepository.GuardarAsync(
                        command.SolicitudId,
                        idUsuarioAlta,
                        trabajadores,
                        cancellationToken
                    );

                return CrearRespuesta(command.SolicitudId, guardados);
            }
            catch (SumariosDomainException exception)
            {
                throw new InvalidException(
                    exception.Message,
                    "Revise la nómina e intente nuevamente.",
                    exception
                );
            }
        }

        private async Task<IReadOnlyCollection<TrabajadorNomina>>
            CargarTrabajadoresAsync(
                long solicitudId,
                Guid idUsuarioAlta,
                CancellationToken cancellationToken
            )
        {
            IReadOnlyCollection<TrabajadorNomina> actuales =
                await _nominaRepository.ObtenerAsync(
                    solicitudId,
                    idUsuarioAlta,
                    cancellationToken
                );

            if (actuales.Any(trabajador => !trabajador.EsManual))
            {
                return actuales;
            }

            Cuit cuit = await _nominaRepository.ObtenerCuitEmpresaAsync(
                solicitudId,
                idUsuarioAlta,
                cancellationToken
            );

            IReadOnlyCollection<TrabajadorNomina> importados;

            try
            {
                importados = await _proveedorNomina.ObtenerNominaPorCuitAsync(
                    cuit,
                    solicitudId,
                    cancellationToken
                );
            }
            catch (HttpRequestException exception)
            {
                throw new InvalidException(
                    "No fue posible obtener la nómina del servicio de empresas. " +
                    exception.Message,
                    "Intente nuevamente cuando el servicio esté disponible.",
                    exception
                );
            }
            catch (OperationCanceledException exception)
                when (!cancellationToken.IsCancellationRequested)
            {
                throw new InvalidException(
                    "El servicio de empresas no respondió a tiempo al consultar la nómina.",
                    "Intente nuevamente.",
                    exception
                );
            }

            return await _nominaRepository.ImportarAsync(
                solicitudId,
                idUsuarioAlta,
                importados,
                cancellationToken
            );
        }

        private static Guid ValidarSolicitudYUsuario(
            long solicitudId,
            string usuario
        )
        {
            if (solicitudId <= 0)
            {
                throw new InvalidException(
                    "La solicitud indicada no es válida.",
                    "Complete primero los datos de la empresa."
                );
            }

            if (!Guid.TryParse(usuario?.Trim(), out Guid idUsuarioAlta) ||
                idUsuarioAlta == Guid.Empty)
            {
                throw new InvalidException(
                    "No fue posible identificar al usuario autenticado.",
                    "Vuelva a iniciar sesión e intente nuevamente."
                );
            }

            return idUsuarioAlta;
        }

        private static TrabajadorNomina CrearTrabajadorSolicitado(
            long solicitudId,
            TrabajadorNominaSolicitudRequest request,
            IReadOnlyDictionary<long, TrabajadorNomina> actuales
        )
        {
            if (request.TrabajadorId < 0)
            {
                throw new SumariosDomainException(
                    "El identificador del trabajador no es válido."
                );
            }

            if (!request.Ingreso.HasValue)
            {
                throw new SumariosDomainException(
                    "Debe informar la fecha de ingreso."
                );
            }

            if (!request.PerteneceAConstruccion.HasValue)
            {
                throw new SumariosDomainException(
                    "Debe indicar si pertenece a la construcción."
                );
            }

            TrabajadorNomina actual = null;

            if (request.TrabajadorId > 0 &&
                !actuales.TryGetValue(request.TrabajadorId, out actual))
            {
                throw new SumariosDomainException(
                    "El trabajador no pertenece a esta solicitud."
                );
            }

            Cuil cuil = ObtenerCuil(request.Cuil);
            bool esManual = actual == null || actual.EsManual;

            TrabajadorNomina trabajador = esManual
                ? TrabajadorNomina.CrearManual(
                    solicitudId,
                    request.ApellidoYNombre,
                    cuil,
                    request.Ingreso.Value,
                    request.Egreso,
                    request.PerteneceAConstruccion.Value
                )
                : TrabajadorNomina.CrearDesdeServicio(
                    solicitudId,
                    request.ApellidoYNombre,
                    cuil,
                    request.Ingreso.Value,
                    request.Egreso,
                    request.PerteneceAConstruccion.Value
                );

            if (actual != null)
            {
                trabajador.AsignarId(actual.Id);
            }

            return trabajador;
        }

        private static Cuil ObtenerCuil(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new SumariosDomainException("Debe informar el CUIL.");
            }

            if (valor.Any(caracter =>
                !(caracter >= '0' && caracter <= '9') &&
                caracter != '-' && !char.IsWhiteSpace(caracter)))
            {
                throw new SumariosDomainException(
                    "El CUIL contiene caracteres inválidos."
                );
            }

            string normalizado = new string(valor
                .Where(caracter => caracter >= '0' && caracter <= '9')
                .ToArray());

            if (normalizado.Length != 11 ||
                !long.TryParse(normalizado, out long numero))
            {
                throw new SumariosDomainException(
                    "El CUIL debe contener 11 números."
                );
            }

            return new Cuil(numero);
        }

        private static NominaSolicitudResponse CrearRespuesta(
            long solicitudId,
            IReadOnlyCollection<TrabajadorNomina> trabajadores
        )
        {
            DateTime? inicio = trabajadores.Count > 0
                ? trabajadores.Min(trabajador => trabajador.Ingreso)
                : (DateTime?)null;

            int? dias = inicio.HasValue
                ? (DateTime.Today - inicio.Value.Date).Days
                : (int?)null;

            return new NominaSolicitudResponse
            {
                SolicitudId = solicitudId,
                CantidadTrabajadores = trabajadores.Count,
                FechaInicioActividades = inicio,
                DiasDesdeInicioActividades = dias,
                RequiereDeclaracionJurada = dias.HasValue && dias.Value > 45,
                Trabajadores = trabajadores.Select(trabajador =>
                    new TrabajadorNominaSolicitudResponse
                    {
                        TrabajadorId = trabajador.Id,
                        ApellidoYNombre = trabajador.ApellidoYNombre,
                        Cuil = trabajador.Cuil.ToString(),
                        Ingreso = trabajador.Ingreso,
                        Egreso = trabajador.Egreso,
                        PerteneceAConstruccion = trabajador.PerteneceAConstruccion,
                        EsManual = trabajador.EsManual
                    }
                ).ToList()
            };
        }
    }
}