using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CargarNominaSolicitudCommand
        : IRequest<NominaSolicitudResponse>
    {
        public long SolicitudId { get; }

        public string IdUsuarioAlta { get; }

        public CargarNominaSolicitudCommand(
            long solicitudId,
            string idUsuarioAlta
        )
        {
            SolicitudId = solicitudId;
            IdUsuarioAlta = idUsuarioAlta;
        }
    }

    public class GuardarNominaSolicitudCommand
        : IRequest<NominaSolicitudResponse>
    {
        public long SolicitudId { get; }

        public GuardarNominaSolicitudRequest Request { get; }

        public string IdUsuarioAlta { get; }

        public GuardarNominaSolicitudCommand(
            long solicitudId,
            GuardarNominaSolicitudRequest request,
            string idUsuarioAlta
        )
        {
            SolicitudId = solicitudId;
            Request = request;
            IdUsuarioAlta = idUsuarioAlta;
        }
    }
}