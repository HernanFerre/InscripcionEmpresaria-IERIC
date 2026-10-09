using System.Threading;
using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IProveedorDatosMaestros
    {
        Task<string> ObtenerActividadesConstruccionAsync(
            CancellationToken cancellationToken = default
        );

        Task<string> ObtenerCaracteresEmpresaAsync(
            CancellationToken cancellationToken = default
        );

        Task<string> ObtenerTiposSociedadAsync(
            CancellationToken cancellationToken = default
        );

        Task<string> ObtenerTiposRepresentantesAsync(
            int tipoSociedad,
            CancellationToken cancellationToken = default
        );

        Task<string> ObtenerLocalidadesAsync(
            string codigoPostal,
            CancellationToken cancellationToken = default
        );
    }
}