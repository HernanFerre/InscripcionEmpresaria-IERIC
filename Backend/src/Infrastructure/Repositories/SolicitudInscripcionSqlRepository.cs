using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class SolicitudInscripcionSqlRepository
        : ISolicitudInscripcionRepository
    {
        private readonly SumariosContext _context;

        public SolicitudInscripcionSqlRepository(
            SumariosContext context
        )
        {
            _context =
                context ??
                throw new ArgumentNullException(
                    nameof(context)
                );
        }

        public async Task GuardarAsync(
            SolicitudInscripcion solicitud
        )
        {
            if (solicitud == null)
            {
                throw new ArgumentNullException(
                    nameof(solicitud)
                );
            }

            DateTime fechaActual = DateTime.Now;

            bool tieneEmpresasIntegrantes =
                solicitud.EmpresasIntegrantes.Count > 0;

            int legacyIdEmpresaPrincipal =
                tieneEmpresasIntegrantes
                    ? 1
                    : 0;

            EmpresaEntity empresaPrincipalEntity =
                CrearEmpresaEntity(
                    solicitud.EmpresaPrincipal,
                    fechaActual,
                    legacyIdEmpresaPrincipal,
                    solicitud.IdUsuarioAlta
                );

            List<(
                EmpresaIntegrante Relacion,
                EmpresaEntity Entidad
            )> empresasIntegrantes =
                new List<(
                    EmpresaIntegrante Relacion,
                    EmpresaEntity Entidad
                )>();

            int legacyIdIntegrante = 2;

            foreach (
                EmpresaIntegrante relacion
                in solicitud.EmpresasIntegrantes
            )
            {
                EmpresaEntity integranteEntity =
                    CrearEmpresaEntity(
                        relacion.Integrante,
                        fechaActual,
                        legacyIdIntegrante,
                        solicitud.IdUsuarioAlta
                    );

                empresasIntegrantes.Add(
                    (
                        relacion,
                        integranteEntity
                    )
                );

                legacyIdIntegrante++;
            }

            await using IDbContextTransaction transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                /*
                 * Primero se guardan las empresas para que
                 * SQL Server genere sus identificadores.
                 */
                _context.Empresas.Add(
                    empresaPrincipalEntity
                );

                foreach (
                    var empresaIntegrante
                    in empresasIntegrantes
                )
                {
                    _context.Empresas.Add(
                        empresaIntegrante.Entidad
                    );
                }

                await _context.SaveChangesAsync();

                /*
                 * Con los Id ya generados se crean las relaciones
                 * de la UTE utilizando Empresa.Id y no el CUIT.
                 */
                foreach (
                    var empresaIntegrante
                    in empresasIntegrantes
                )
                {
                    _context.EmpresasCuit.Add(
                        new EmpresaIntegranteEntity
                        {
                            IdEmpresa =
                                empresaPrincipalEntity.Id,

                            IdEmpresaIntegrante =
                                empresaIntegrante
                                    .Entidad
                                    .Id
                        }
                    );
                }

                SolicitudInscripcionEntity solicitudEntity =
                    new SolicitudInscripcionEntity
                    {
                        Idempresa =
                            empresaPrincipalEntity.Id,

                        IdUsuarioAlta =
                            solicitud.IdUsuarioAlta,

                        Comentarios =
                            solicitud.Comentarios,

                        Um = null,
                        Fa = fechaActual,
                        Fm = fechaActual
                    };

                _context.SolicitudesInscripcion.Add(
                    solicitudEntity
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                /*
                 * Los identificadores se asignan al dominio
                 * solamente después de confirmar la transacción.
                 */
                solicitud.EmpresaPrincipal.AsignarId(
                    empresaPrincipalEntity.Id
                );

                foreach (
                    var empresaIntegrante
                    in empresasIntegrantes
                )
                {
                    empresaIntegrante
                        .Relacion
                        .Integrante
                        .AsignarId(
                            empresaIntegrante
                                .Entidad
                                .Id
                        );
                }

                solicitud.AsignarId(
                    solicitudEntity.Id
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task GuardarRepresentantesAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<Representante> representantes,
            CancellationToken cancellationToken = default
        )
        {
            if (solicitudId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(solicitudId)
                );
            }

            if (idUsuarioAlta == Guid.Empty)
            {
                throw new ArgumentException(
                    "El usuario de la solicitud no es válido.",
                    nameof(idUsuarioAlta)
                );
            }

            if (representantes == null)
            {
                throw new ArgumentNullException(
                    nameof(representantes)
                );
            }

            SolicitudInscripcionEntity solicitudEntity =
                await _context.SolicitudesInscripcion
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        solicitud =>
                            solicitud.Id == solicitudId &&
                            solicitud.IdUsuarioAlta ==
                                idUsuarioAlta,
                        cancellationToken
                    );

            if (solicitudEntity == null)
            {
                throw new SumariosDomainException(
                    "No se encontró la solicitud indicada " +
                    "para el usuario autenticado."
                );
            }

            List<long> empresasIntegrantes =
                await _context.EmpresasCuit
                    .AsNoTracking()
                    .Where(
                        relacion =>
                            relacion.IdEmpresa ==
                            solicitudEntity.Idempresa
                    )
                    .Select(
                        relacion =>
                            relacion.IdEmpresaIntegrante
                    )
                    .ToListAsync(cancellationToken);

            HashSet<long> empresasPermitidas =
                empresasIntegrantes.Count > 0
                    ? new HashSet<long>(
                        empresasIntegrantes
                    )
                    : new HashSet<long>
                    {
                        solicitudEntity.Idempresa
                    };

            foreach (
                Representante representante
                in representantes
            )
            {
                if (
                    !empresasPermitidas.Contains(
                        representante.EmpresaId
                    )
                )
                {
                    throw new SumariosDomainException(
                        "La empresa seleccionada para un " +
                        "representante no pertenece a la solicitud."
                    );
                }
            }

            await using IDbContextTransaction transaction =
                await _context.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                List<SolicitudRepresentanteEntity>
                    relacionesActuales =
                        await _context
                            .SolicitudesRepresentantes
                            .Where(
                                relacion =>
                                    relacion.IdSolicitud ==
                                    solicitudId
                            )
                            .ToListAsync(
                                cancellationToken
                            );

                List<long> empresasSolicitadas =
                    representantes
                        .Select(
                            representante =>
                                representante.EmpresaId
                        )
                        .Distinct()
                        .ToList();

                List<long> cuilesSolicitados =
                    representantes
                        .Select(
                            representante =>
                                representante.Cuil
                                    .ToInt64()
                        )
                        .Distinct()
                        .ToList();

                List<RepresentanteEntity>
                    representantesExistentes =
                        empresasSolicitadas.Count == 0
                            ? new List<RepresentanteEntity>()
                            : await _context.Representantes
                                .Where(
                                    representante =>
                                        empresasSolicitadas
                                            .Contains(
                                                representante
                                                    .IdEmpresa
                                            ) &&
                                        cuilesSolicitados
                                            .Contains(
                                                representante
                                                    .Cuil
                                            )
                                )
                                .ToListAsync(
                                    cancellationToken
                                );

                List<(
                    Representante Dominio,
                    RepresentanteEntity Entidad
                )> representantesGuardados =
                    new List<(
                        Representante Dominio,
                        RepresentanteEntity Entidad
                    )>();

                foreach (
                    Representante representante
                    in representantes
                )
                {
                    long cuil =
                        representante.Cuil.ToInt64();

                    RepresentanteEntity representanteEntity =
                        representantesExistentes
                            .FirstOrDefault(
                                existente =>
                                    existente.IdEmpresa ==
                                        representante
                                            .EmpresaId &&
                                    existente.Cuil == cuil
                            );

                    if (representanteEntity == null)
                    {
                        representanteEntity =
                            new RepresentanteEntity
                            {
                                IdEmpresa =
                                    representante
                                        .EmpresaId,

                                Cuil = cuil
                            };

                        _context.Representantes.Add(
                            representanteEntity
                        );

                        representantesExistentes.Add(
                            representanteEntity
                        );
                    }

                    representanteEntity.Apellido =
                        representante.Apellido;

                    representanteEntity.Nombre =
                        representante.Nombre;

                    representanteEntity
                        .IdTipoRepresentante =
                            representante
                                .TipoRepresentanteId;

                    representanteEntity.Correo =
                        representante.Correo;

                    representanteEntity.Telefono =
                        representante.Telefono;

                    representantesGuardados.Add(
                        (
                            representante,
                            representanteEntity
                        )
                    );
                }

                await _context.SaveChangesAsync(
                    cancellationToken
                );

                HashSet<long> identificadoresDeseados =
                    representantesGuardados
                        .Select(
                            item => item.Entidad.Id
                        )
                        .ToHashSet();

                List<SolicitudRepresentanteEntity>
                    relacionesAEliminar =
                        relacionesActuales
                            .Where(
                                relacion =>
                                    !identificadoresDeseados
                                        .Contains(
                                            relacion
                                                .IdRepresentante
                                        )
                            )
                            .ToList();

                _context
                    .SolicitudesRepresentantes
                    .RemoveRange(
                        relacionesAEliminar
                    );

                HashSet<long> identificadoresRelacionados =
                    relacionesActuales
                        .Select(
                            relacion =>
                                relacion.IdRepresentante
                        )
                        .ToHashSet();

                foreach (
                    var representanteGuardado
                    in representantesGuardados
                )
                {
                    if (
                        identificadoresRelacionados
                            .Contains(
                                representanteGuardado
                                    .Entidad
                                    .Id
                            )
                    )
                    {
                        continue;
                    }

                    _context
                        .SolicitudesRepresentantes
                        .Add(
                            new SolicitudRepresentanteEntity
                            {
                                IdSolicitud =
                                    solicitudId,

                                IdRepresentante =
                                    representanteGuardado
                                        .Entidad
                                        .Id
                            }
                        );
                }

                await _context.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );

                foreach (
                    var representanteGuardado
                    in representantesGuardados
                )
                {
                    representanteGuardado
                        .Dominio
                        .AsignarId(
                            representanteGuardado
                                .Entidad
                                .Id
                        );
                }
            }
            catch
            {
                await transaction.RollbackAsync(
                    cancellationToken
                );

                throw;
            }
        }

        private static EmpresaEntity CrearEmpresaEntity(
            Empresa empresa,
            DateTime fechaActual,
            int legacyId,
            Guid idUsuarioAlta
        )
        {
            return new EmpresaEntity
            {
                RazonSocial =
                    empresa.RazonSocial,

                Cuit =
                    empresa.Cuit.ToInt64(),

                EsCooperativa =
                    empresa.TipoSociedadId == 10 ||
                    empresa.TipoSociedadId == 23,

                EstadoActivo = true,

                LegacyId = legacyId,

                Activo = true,

                Fa = fechaActual,
                IdUsuarioAlta = idUsuarioAlta,
                Fm = fechaActual,
                Um = null,

                Calle =
                    empresa.Calle ??
                    string.Empty,

                Numero =
                    empresa.Numero ??
                    string.Empty,

                Piso =
                    empresa.Piso,

                DeptoOficina =
                    empresa.DepartamentoOficina,

                CodigoPostal =
                    empresa.CodigoPostal ??
                    string.Empty,

                IdProvincia =
                    empresa.IdProvincia,

                IdLocalidad =
                    empresa.IdLocalidad,

                Correo =
                    empresa.Correo ??
                    string.Empty,

                Telefono =
                    empresa.Telefono,

                IdActividadsolicitud =
                    empresa.ActividadId ?? 0,

                IdCaracter =
                    empresa.CaracterId ?? 0,

                IdTipoSoc =
                    empresa.TipoSociedadId,

                DDJJ = false
            };
        }
    }
}