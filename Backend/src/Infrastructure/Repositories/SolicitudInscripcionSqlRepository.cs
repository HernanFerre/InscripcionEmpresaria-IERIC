using System;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion;

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

            EmpresaEntity empresaPrincipalEntity =
                CrearEmpresaEntity(
                    solicitud.EmpresaPrincipal,
                    fechaActual
                );

            _context.Empresas.Add(
                empresaPrincipalEntity
            );

            foreach (
                EmpresaIntegrante relacion
                in solicitud.EmpresasIntegrantes
            )
            {
                EmpresaEntity integranteEntity =
                    CrearEmpresaEntity(
                        relacion.Integrante,
                        fechaActual
                    );

                _context.Empresas.Add(
                    integranteEntity
                );

                _context.EmpresasCuit.Add(
                    new EmpresaIntegranteEntity
                    {
                        IdEmpresa =
                            relacion
                                .CuitEmpresaPrincipal
                                .ToInt64(),

                        IdEmpresaIntegrante =
                            relacion
                                .CuitEmpresaIntegrante
                                .ToInt64()
                    }
                );
            }

            SolicitudInscripcionEntity solicitudEntity =
                new SolicitudInscripcionEntity
                {
                    Idempresa =
                        solicitud
                            .EmpresaPrincipal
                            .Cuit
                            .ToInt64(),

                    UsuarioId = solicitud.UsuarioId,
                    Comentarios = solicitud.Comentarios,
                    Ua = null,
                    Um = null,
                    Fa = fechaActual,
                    Fm = fechaActual
                };

            _context.SolicitudesInscripcion.Add(
                solicitudEntity
            );

            /*
             * Un único SaveChangesAsync hace que EF Core
             * guarde la empresa, sus integrantes, las relaciones
             * y la solicitud dentro de una misma transacción.
             */
            await _context.SaveChangesAsync();

            solicitud.AsignarId(
                solicitudEntity.Id
            );
        }

        private static EmpresaEntity CrearEmpresaEntity(
            Empresa empresa,
            DateTime fechaActual
        )
        {
            return new EmpresaEntity
            {
                /*
                 * El esquema recibido exige Id, pero no lo define
                 * como PK, Identity ni recibe ese dato desde la UI.
                 */
                Id = 0,

                RazonSocial = empresa.RazonSocial,
                Cuit = empresa.Cuit.ToInt64(),

                EsCooperativa =
                    empresa.TipoSociedadId == 10 ||
                    empresa.TipoSociedadId == 23,

                EstadoActivo = true,
                LegacyId = 0,
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

                Piso = empresa.Piso,

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

                Telefono = empresa.Telefono,

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