using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public interface ISolicitudInscripcionRepository
    {
        Task GuardarAsync(
            SolicitudInscripcion solicitud
        );

        Task GuardarRepresentantesAsync(
            long solicitudId,
            Guid idUsuarioAlta,
            IReadOnlyCollection<Representante> representantes,
            CancellationToken cancellationToken = default
        );
    }
}