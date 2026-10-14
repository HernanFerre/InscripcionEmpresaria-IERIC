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
                throw new ArgumentNullException(nameof(solicitud));
            }

            DateTime fechaActual = DateTime.Now;
            long cuitPrincipal = solicitud.EmpresaPrincipal.Cuit.ToInt64();
            bool tieneIntegrantes = solicitud.EmpresasIntegrantes.Count > 0;
            int legacyIdPrincipal = tieneIntegrantes ? 1 : 0;

            await using IDbContextTransaction transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                EmpresaEntity principal = await _context.Empresas
                    .SingleOrDefaultAsync(empresa => empresa.Cuit == cuitPrincipal);

                SolicitudInscripcionEntity solicitudEntity = null;
                bool huboCambios = false;
                HashSet<long> empresasConCambioDeSociedad = new HashSet<long>();

                if (principal == null)
                {
                    principal = CrearEmpresaEntity(
                        solicitud.EmpresaPrincipal,
                        fechaActual,
                        legacyIdPrincipal,
                        solicitud.IdUsuarioAlta
                    );

                    _context.Empresas.Add(principal);
                    huboCambios = true;
                }
                else
                {
                    solicitudEntity = await _context.SolicitudesInscripcion
                        .SingleOrDefaultAsync(item => item.Idempresa == principal.Id);

                    if (solicitudEntity == null ||
                        solicitudEntity.IdUsuarioAlta != solicitud.IdUsuarioAlta)
                    {
                        throw new SumariosDomainException(
                            "El CUIT indicado ya está asociado a otra inscripción " +
                            "y no puede modificarse desde esta solicitud."
                        );
                    }

                    if (principal.IdTipoSoc != solicitud.EmpresaPrincipal.TipoSociedadId)
                    {
                        empresasConCambioDeSociedad.Add(principal.Id);
                    }

                    huboCambios |= ActualizarEmpresaEntity(
                        principal,
                        solicitud.EmpresaPrincipal,
                        fechaActual,
                        legacyIdPrincipal
                    );
                }

                List<EmpresaIntegranteEntity> relacionesActuales = principal.Id > 0
                    ? await _context.EmpresasCuit
                        .Where(relacion => relacion.IdEmpresa == principal.Id)
                        .ToListAsync()
                    : new List<EmpresaIntegranteEntity>();

                List<long> cuitsIntegrantes = solicitud.EmpresasIntegrantes
                    .Select(relacion => relacion.Integrante.Cuit.ToInt64())
                    .ToList();

                List<EmpresaEntity> integrantesExistentes = cuitsIntegrantes.Count > 0
                    ? await _context.Empresas
                        .Where(empresa => cuitsIntegrantes.Contains(empresa.Cuit))
                        .ToListAsync()
                    : new List<EmpresaEntity>();

                List<long> idsExistentes = integrantesExistentes
                    .Select(empresa => empresa.Id)
                    .ToList();

                HashSet<long> integrantesDeOtraInscripcion = new HashSet<long>();

                if (idsExistentes.Count > 0)
                {
                    List<long> empresasPrincipales = await _context.SolicitudesInscripcion
                        .Where(item => idsExistentes.Contains(item.Idempresa))
                        .Select(item => item.Idempresa)
                        .ToListAsync();

                    List<long> integrantesDeOtraEmpresa = await _context.EmpresasCuit
                        .Where(relacion =>
                            idsExistentes.Contains(relacion.IdEmpresaIntegrante) &&
                            relacion.IdEmpresa != principal.Id)
                        .Select(relacion => relacion.IdEmpresaIntegrante)
                        .ToListAsync();

                    integrantesDeOtraInscripcion.UnionWith(empresasPrincipales);
                    integrantesDeOtraInscripcion.UnionWith(integrantesDeOtraEmpresa);
                }

                List<(EmpresaIntegrante Relacion, EmpresaEntity Entidad)> integrantes =
                    new List<(EmpresaIntegrante Relacion, EmpresaEntity Entidad)>();

                int legacyIdIntegrante = 2;

                foreach (EmpresaIntegrante relacion in solicitud.EmpresasIntegrantes)
                {
                    long cuitIntegrante = relacion.Integrante.Cuit.ToInt64();

                    EmpresaEntity integrante = integrantesExistentes
                        .FirstOrDefault(empresa => empresa.Cuit == cuitIntegrante);

                    if (integrante == null)
                    {
                        integrante = CrearEmpresaEntity(
                            relacion.Integrante,
                            fechaActual,
                            legacyIdIntegrante,
                            solicitud.IdUsuarioAlta
                        );

                        _context.Empresas.Add(integrante);
                        huboCambios = true;
                    }
                    else
                    {
                        if (integrante.IdUsuarioAlta != solicitud.IdUsuarioAlta ||
                            integrantesDeOtraInscripcion.Contains(integrante.Id))
                        {
                            throw new SumariosDomainException(
                                $"La integrante con CUIT {cuitIntegrante} ya está " +
                                "asociada a otra inscripción y no puede modificarse aquí."
                            );
                        }

                        if (integrante.IdTipoSoc != relacion.Integrante.TipoSociedadId)
                        {
                            empresasConCambioDeSociedad.Add(integrante.Id);
                        }

                        huboCambios |= ActualizarEmpresaEntity(
                            integrante,
                            relacion.Integrante,
                            fechaActual,
                            legacyIdIntegrante
                        );
                    }

                    integrantes.Add((relacion, integrante));
                    legacyIdIntegrante++;
                }

                // Genera los IDs de las empresas nuevas y actualiza solo las modificadas.
                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync();
                }

                HashSet<long> idsDeseados = integrantes
                    .Select(item => item.Entidad.Id)
                    .ToHashSet();

                List<EmpresaIntegranteEntity> relacionesAEliminar = relacionesActuales
                    .Where(relacion => !idsDeseados.Contains(relacion.IdEmpresaIntegrante))
                    .ToList();

                _context.EmpresasCuit.RemoveRange(relacionesAEliminar);
                huboCambios |= relacionesAEliminar.Count > 0;

                HashSet<long> idsRelacionados = relacionesActuales
                    .Select(relacion => relacion.IdEmpresaIntegrante)
                    .ToHashSet();

                foreach (var integrante in integrantes)
                {
                    if (idsRelacionados.Contains(integrante.Entidad.Id))
                    {
                        continue;
                    }

                    _context.EmpresasCuit.Add(new EmpresaIntegranteEntity
                    {
                        IdEmpresa = principal.Id,
                        IdEmpresaIntegrante = integrante.Entidad.Id
                    });

                    huboCambios = true;
                }

                if (solicitudEntity == null)
                {
                    solicitudEntity = new SolicitudInscripcionEntity
                    {
                        Idempresa = principal.Id,
                        IdUsuarioAlta = solicitud.IdUsuarioAlta,
                        Comentarios = solicitud.Comentarios,
                        Um = null,
                        Fa = fechaActual,
                        Fm = fechaActual
                    };

                    _context.SolicitudesInscripcion.Add(solicitudEntity);
                }
                else
                {
                    HashSet<long> empresasPermitidas = tieneIntegrantes
                        ? idsDeseados
                        : new HashSet<long> { principal.Id };

                    // Desvincula los representantes de integrantes retiradas o
                    // de empresas cuyo cambio de sociedad exige revisar el cargo.
                    List<SolicitudRepresentanteEntity> representantesARevisar =
                        await (
                            from relacion in _context.SolicitudesRepresentantes
                            join representante in _context.Representantes
                                on relacion.IdRepresentante equals representante.Id
                            where relacion.IdSolicitud == solicitudEntity.Id &&
                                (!empresasPermitidas.Contains(representante.IdEmpresa) ||
                                 empresasConCambioDeSociedad.Contains(representante.IdEmpresa))
                            select relacion
                        ).ToListAsync();

                    _context.SolicitudesRepresentantes.RemoveRange(representantesARevisar);
                    huboCambios |= representantesARevisar.Count > 0;

                    if (huboCambios)
                    {
                        solicitudEntity.Fm = fechaActual;
                    }
                }

                if (_context.ChangeTracker.HasChanges())
                {
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                if (solicitud.EmpresaPrincipal.Id == 0)
                {
                    solicitud.EmpresaPrincipal.AsignarId(principal.Id);
                }

                foreach (var integrante in integrantes)
                {
                    if (integrante.Relacion.Integrante.Id == 0)
                    {
                        integrante.Relacion.Integrante.AsignarId(integrante.Entidad.Id);
                    }
                }

                if (solicitud.Id == 0)
                {
                    solicitud.AsignarId(solicitudEntity.Id);
                }
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

        private bool ActualizarEmpresaEntity(
            EmpresaEntity entidad,
            Empresa empresa,
            DateTime fechaActual,
            int legacyId
        )
        {
            entidad.RazonSocial = empresa.RazonSocial;
            entidad.IdTipoSoc = empresa.TipoSociedadId;
            entidad.EsCooperativa =
                empresa.TipoSociedadId == 10 ||
                empresa.TipoSociedadId == 23;
            entidad.LegacyId = legacyId;

            // Las integrantes solo aportan CUIT, razón social y tipo de sociedad.
            if (empresa.TieneDatosCompletos)
            {
                entidad.Calle = empresa.Calle ?? string.Empty;
                entidad.Numero = empresa.Numero ?? string.Empty;
                entidad.Piso = empresa.Piso;
                entidad.DeptoOficina = empresa.DepartamentoOficina;
                entidad.CodigoPostal = empresa.CodigoPostal ?? string.Empty;
                entidad.IdProvincia = empresa.IdProvincia;
                entidad.IdLocalidad = empresa.IdLocalidad;
                entidad.Correo = empresa.Correo ?? string.Empty;
                entidad.Telefono = empresa.Telefono;
                entidad.IdActividadsolicitud = empresa.ActividadId ?? 0;
                entidad.IdCaracter = empresa.CaracterId ?? 0;
            }

            _context.ChangeTracker.DetectChanges();

            if (_context.Entry(entidad).State != EntityState.Modified)
            {
                return false;
            }

            entidad.Fm = fechaActual;
            return true;
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