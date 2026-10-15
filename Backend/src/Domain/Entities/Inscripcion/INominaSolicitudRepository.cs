using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public interface INominaSolicitudRepository
    {
        Task<Cuit> ObtenerCuitEmpresaAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyCollection<TrabajadorNomina>> ObtenerAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyCollection<TrabajadorNomina>> ImportarAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<TrabajadorNomina> trabajadoresDelServicio,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyCollection<TrabajadorNomina>> GuardarAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<TrabajadorNomina> trabajadores,
            CancellationToken cancellationToken = default
        );
    }
}