using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class GuardarRepresentantesSolicitudCommand
        : IRequest<GuardarRepresentantesSolicitudResponse>
    {
        public long SolicitudId { get; }

        public GuardarRepresentantesSolicitudRequest Request
        { get; }

        public string IdUsuarioAlta { get; }

        public GuardarRepresentantesSolicitudCommand(
            long solicitudId,
            GuardarRepresentantesSolicitudRequest request,
            string idUsuarioAlta
        )
        {
            SolicitudId = solicitudId;
            Request = request;
            IdUsuarioAlta = idUsuarioAlta;
        }
    }
}