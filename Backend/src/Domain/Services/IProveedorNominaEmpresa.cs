using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities.Inscripcion;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IProveedorNominaEmpresa
    {
        Task<IReadOnlyCollection<TrabajadorNomina>>
            ObtenerNominaPorCuitAsync(
                Cuit cuitEmpresa,
                long solicitudId,
                CancellationToken cancellationToken = default
            );
    }
}