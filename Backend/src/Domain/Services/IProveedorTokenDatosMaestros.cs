using System.Threading;
using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IProveedorTokenDatosMaestros
    {
        Task<string> ObtenerTokenAsync(
            CancellationToken cancellationToken = default
        );

        void InvalidarToken();
    }
}