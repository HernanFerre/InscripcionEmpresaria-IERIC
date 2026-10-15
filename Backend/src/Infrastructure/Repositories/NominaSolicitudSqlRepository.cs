using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion;
using Microsoft.EntityFrameworkCore;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class NominaSolicitudSqlRepository : INominaSolicitudRepository
    {
        private readonly SumariosContext _context;
        private readonly IPrincipal _principal;

        public NominaSolicitudSqlRepository(
            SumariosContext context,
            IPrincipal principal
        )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        public async Task<Cuit> ObtenerCuitEmpresaAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            CancellationToken cancellationToken = default
        )
        {
            ValidarIdentificadores(solicitudId, idUsuarioAlta);

            long? cuit = await (
                from solicitud in _context.SolicitudesInscripcion.AsNoTracking()
                join empresa in _context.Empresas.AsNoTracking()
                    on solicitud.Idempresa equals empresa.Id
                where solicitud.Id == solicitudId &&
                    solicitud.IdUsuarioAlta == idUsuarioAlta
                select (long?)empresa.Cuit
            ).SingleOrDefaultAsync(cancellationToken);

            if (!cuit.HasValue)
            {
                throw new SumariosDomainException(
                    "No se encontró la solicitud indicada para el usuario autenticado."
                );
            }

            return new Cuit(cuit.Value);
        }

        public async Task<IReadOnlyCollection<TrabajadorNomina>> ObtenerAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            CancellationToken cancellationToken = default
        )
        {
            ValidarIdentificadores(solicitudId, idUsuarioAlta);

            bool perteneceAlUsuario = await _context.SolicitudesInscripcion
                .AsNoTracking()
                .AnyAsync(
                    solicitud => solicitud.Id == solicitudId &&
                        solicitud.IdUsuarioAlta == idUsuarioAlta,
                    cancellationToken
                );

            if (!perteneceAlUsuario)
            {
                throw new SumariosDomainException(
                    "No se encontró la solicitud indicada para el usuario autenticado."
                );
            }

            List<TrabajadorNominaEntity> entidades = await _context
                .Set<TrabajadorNominaEntity>()
                .AsNoTracking()
                .Where(trabajador => trabajador.IdSolicitud == solicitudId)
                .ToListAsync(cancellationToken);

            return ConvertirYOrdenar(entidades);
        }

        public async Task<IReadOnlyCollection<TrabajadorNomina>> ImportarAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<TrabajadorNomina> trabajadoresDelServicio,
            CancellationToken cancellationToken = default
        )
        {
            ValidarIdentificadores(solicitudId, idUsuarioAlta);
            ValidarTrabajadores(solicitudId, trabajadoresDelServicio);

            if (trabajadoresDelServicio.Any(trabajador =>
                trabajador.EsManual || trabajador.Id != 0))
            {
                throw new SumariosDomainException(
                    "La importación debe contener trabajadores nuevos del servicio."
                );
            }

            await using var transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);

            SolicitudInscripcionEntity solicitud =
                await ObtenerSolicitudParaActualizarAsync(
                    solicitudId,
                    idUsuarioAlta,
                    cancellationToken
                );

            List<TrabajadorNominaEntity> actuales = await _context
                .Set<TrabajadorNominaEntity>()
                .Where(trabajador => trabajador.IdSolicitud == solicitudId)
                .ToListAsync(cancellationToken);

            if (actuales.Any(trabajador => !trabajador.EsManual))
            {
                await transaction.CommitAsync(cancellationToken);
                return ConvertirYOrdenar(actuales);
            }

            List<TrabajadorNominaEntity> nuevas = trabajadoresDelServicio
                .Select(CrearEntidad)
                .ToList();

            if (nuevas.Count > 0)
            {
                _context.Set<TrabajadorNominaEntity>().AddRange(nuevas);
                RegistrarModificacion(solicitud);
                await _context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            actuales.AddRange(nuevas);
            return ConvertirYOrdenar(actuales);
        }

        public async Task<IReadOnlyCollection<TrabajadorNomina>> GuardarAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<TrabajadorNomina> trabajadores,
            CancellationToken cancellationToken = default
        )
        {
            ValidarIdentificadores(solicitudId, idUsuarioAlta);
            ValidarTrabajadores(solicitudId, trabajadores);

            List<long> idsRecibidos = trabajadores
                .Where(trabajador => trabajador.Id > 0)
                .Select(trabajador => trabajador.Id)
                .ToList();

            HashSet<long> idsConservados = new HashSet<long>(idsRecibidos);

            if (idsConservados.Count != idsRecibidos.Count)
            {
                throw new SumariosDomainException(
                    "La nómina contiene identificadores de trabajadores repetidos."
                );
            }

            await using var transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);

            SolicitudInscripcionEntity solicitud =
                await ObtenerSolicitudParaActualizarAsync(
                    solicitudId,
                    idUsuarioAlta,
                    cancellationToken
                );

            List<TrabajadorNominaEntity> actuales = await _context
                .Set<TrabajadorNominaEntity>()
                .Where(trabajador => trabajador.IdSolicitud == solicitudId)
                .ToListAsync(cancellationToken);

            Dictionary<long, TrabajadorNominaEntity> porId = actuales
                .ToDictionary(trabajador => trabajador.Id);

            foreach (TrabajadorNomina trabajador in trabajadores)
            {
                if (trabajador.Id == 0)
                {
                    if (!trabajador.EsManual)
                    {
                        throw new SumariosDomainException(
                            "Los trabajadores nuevos deben agregarse como carga manual."
                        );
                    }

                    continue;
                }

                if (!porId.TryGetValue(trabajador.Id, out var actual))
                {
                    throw new SumariosDomainException(
                        $"El trabajador con CUIL {trabajador.Cuil} " +
                        "no pertenece a esta solicitud."
                    );
                }

                if (actual.EsManual != trabajador.EsManual)
                {
                    throw new SumariosDomainException(
                        $"No se puede cambiar el origen del trabajador " +
                        $"con CUIL {trabajador.Cuil}."
                    );
                }

                if (!actual.EsManual && !CoincidenDatos(actual, trabajador))
                {
                    throw new SumariosDomainException(
                        $"El trabajador {actual.ApellidoYNombre}, CUIL {actual.Cuil}, " +
                        "proviene del servicio. Solo puede cambiarse " +
                        "su pertenencia a la construcción."
                    );
                }
            }

            TrabajadorNominaEntity servicioEliminado = actuales.FirstOrDefault(
                trabajador => !trabajador.EsManual &&
                    !idsConservados.Contains(trabajador.Id)
            );

            if (servicioEliminado != null)
            {
                throw new SumariosDomainException(
                    $"El trabajador {servicioEliminado.ApellidoYNombre}, " +
                    $"CUIL {servicioEliminado.Cuil}, proviene del servicio " +
                    "y no puede eliminarse."
                );
            }

            List<TrabajadorNominaEntity> eliminados = actuales
                .Where(trabajador => trabajador.EsManual &&
                    !idsConservados.Contains(trabajador.Id))
                .ToList();

            _context.Set<TrabajadorNominaEntity>().RemoveRange(eliminados);

            List<TrabajadorNominaEntity> guardados = actuales
                .Where(trabajador => idsConservados.Contains(trabajador.Id))
                .ToList();

            bool huboCambios = eliminados.Count > 0;

            foreach (TrabajadorNomina trabajador in trabajadores.Reverse())
            {
                if (trabajador.Id == 0)
                {
                    TrabajadorNominaEntity nueva = CrearEntidad(trabajador);
                    _context.Set<TrabajadorNominaEntity>().Add(nueva);
                    guardados.Add(nueva);
                    huboCambios = true;
                    continue;
                }

                TrabajadorNominaEntity actual = porId[trabajador.Id];

                if (actual.EsManual && !CoincidenDatos(actual, trabajador))
                {
                    actual.ApellidoYNombre = trabajador.ApellidoYNombre;
                    actual.Cuil = trabajador.Cuil.ToString();
                    actual.Ingreso = trabajador.Ingreso;
                    actual.Egreso = trabajador.Egreso;
                    huboCambios = true;
                }

                if (actual.PerteneceAConstr != trabajador.PerteneceAConstruccion)
                {
                    actual.PerteneceAConstr = trabajador.PerteneceAConstruccion;
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                RegistrarModificacion(solicitud);
                await _context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return ConvertirYOrdenar(guardados);
        }

        private async Task<SolicitudInscripcionEntity>
            ObtenerSolicitudParaActualizarAsync(
                long solicitudId,
                Guid idUsuarioAlta,
                CancellationToken cancellationToken
            )
        {
            SolicitudInscripcionEntity solicitud = await _context
                .SolicitudesInscripcion
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM [dbo].[SolicitudInscripcionDigitalEmpresaria]
                        WITH (UPDLOCK, ROWLOCK)
                    WHERE [Id] = {solicitudId}
                        AND [IdUsuarioAlta] = {idUsuarioAlta}")
                .SingleOrDefaultAsync(cancellationToken);

            if (solicitud == null)
            {
                throw new SumariosDomainException(
                    "No se encontró la solicitud indicada para el usuario autenticado."
                );
            }

            return solicitud;
        }

        private void RegistrarModificacion(SolicitudInscripcionEntity solicitud)
        {
            string usuario = _principal.Identity?.Name?.Trim();

            if (string.IsNullOrWhiteSpace(usuario) || usuario.Length > 254)
            {
                throw new SumariosDomainException(
                    "No fue posible obtener un nombre de usuario válido " +
                    "para registrar la modificación."
                );
            }

            solicitud.Um = usuario;
            solicitud.Fm = DateTime.Now;
        }

        private static void ValidarIdentificadores(
            long solicitudId,
            Guid idUsuarioAlta
        )
        {
            if (solicitudId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(solicitudId));
            }

            if (idUsuarioAlta == Guid.Empty)
            {
                throw new SumariosDomainException(
                    "El usuario de la solicitud no es válido."
                );
            }
        }

        private static void ValidarTrabajadores(
            long solicitudId,
            IReadOnlyCollection<TrabajadorNomina> trabajadores
        )
        {
            if (trabajadores == null)
            {
                throw new ArgumentNullException(nameof(trabajadores));
            }

            if (trabajadores.Any(trabajador => trabajador == null ||
                trabajador.SolicitudId != solicitudId || trabajador.Id < 0))
            {
                throw new SumariosDomainException(
                    "La nómina contiene trabajadores inválidos " +
                    "o asociados a otra solicitud."
                );
            }
        }

        private static bool CoincidenDatos(
            TrabajadorNominaEntity entidad,
            TrabajadorNomina trabajador
        )
        {
            return entidad.ApellidoYNombre == trabajador.ApellidoYNombre &&
                entidad.Cuil == trabajador.Cuil.ToString() &&
                entidad.Ingreso.Date == trabajador.Ingreso &&
                entidad.Egreso?.Date == trabajador.Egreso;
        }

        private static TrabajadorNominaEntity CrearEntidad(
            TrabajadorNomina trabajador
        )
        {
            return new TrabajadorNominaEntity
            {
                IdSolicitud = trabajador.SolicitudId,
                ApellidoYNombre = trabajador.ApellidoYNombre,
                Cuil = trabajador.Cuil.ToString(),
                Ingreso = trabajador.Ingreso,
                Egreso = trabajador.Egreso,
                PerteneceAConstr = trabajador.PerteneceAConstruccion,
                EsManual = trabajador.EsManual
            };
        }

        private static IReadOnlyCollection<TrabajadorNomina> ConvertirYOrdenar(
            IEnumerable<TrabajadorNominaEntity> entidades
        )
        {
            return entidades
                .OrderByDescending(entidad => entidad.EsManual)
                .ThenByDescending(entidad => entidad.EsManual ? entidad.Id : 0)
                .ThenByDescending(entidad => entidad.Ingreso)
                .ThenBy(
                    entidad => entidad.ApellidoYNombre,
                    StringComparer.OrdinalIgnoreCase
                )
                .ThenBy(entidad => entidad.Id)
                .Select(ConvertirADominio)
                .ToList();
        }

        private static TrabajadorNomina ConvertirADominio(
            TrabajadorNominaEntity entidad
        )
        {
            if (!long.TryParse(entidad.Cuil, out long numeroCuil))
            {
                throw new SumariosDomainException(
                    $"El trabajador {entidad.ApellidoYNombre} tiene " +
                    "un CUIL inválido en la nómina guardada."
                );
            }

            Cuil cuil = new Cuil(numeroCuil);

            TrabajadorNomina trabajador = entidad.EsManual
                ? TrabajadorNomina.CrearManual(
                    entidad.IdSolicitud,
                    entidad.ApellidoYNombre,
                    cuil,
                    entidad.Ingreso,
                    entidad.Egreso,
                    entidad.PerteneceAConstr
                )
                : TrabajadorNomina.CrearDesdeServicio(
                    entidad.IdSolicitud,
                    entidad.ApellidoYNombre,
                    cuil,
                    entidad.Ingreso,
                    entidad.Egreso,
                    entidad.PerteneceAConstr
                );

            trabajador.AsignarId(entidad.Id);
            return trabajador;
        }
    }
}