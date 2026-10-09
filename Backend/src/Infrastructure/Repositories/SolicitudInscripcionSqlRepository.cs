using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
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
                    legacyIdEmpresaPrincipal
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
                        legacyIdIntegrante
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

                        UsuarioId =
                            solicitud.UsuarioId,

                        Comentarios =
                            solicitud.Comentarios,

                        Ua = null,
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

        private static EmpresaEntity CrearEmpresaEntity(
            Empresa empresa,
            DateTime fechaActual,
            int legacyId
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
                Ua = null,
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

                Provincia =
                    empresa.Provincia ??
                    string.Empty,

                Localidad =
                    empresa.Localidad ??
                    string.Empty,

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