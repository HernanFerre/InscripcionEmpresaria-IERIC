using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IProveedorCuilesEmpresa
    {
        Task<IReadOnlyCollection<Cuil>> ObtenerPorCuitAsync(
            Cuit cuitEmpresa,
            int limite,
            CancellationToken cancellationToken = default
        );
    }
}