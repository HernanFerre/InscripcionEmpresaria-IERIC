using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.ValueObjects;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IProveedorEstadoEmpresa
    {
        Task<EstadoInscripcionEmpresa>
            ObtenerPorCuitAsync(
                Cuit cuitEmpresa,
                CancellationToken cancellationToken = default
            );
    }
}