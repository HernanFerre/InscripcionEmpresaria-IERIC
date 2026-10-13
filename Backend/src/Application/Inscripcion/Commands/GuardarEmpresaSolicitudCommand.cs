using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class GuardarEmpresaSolicitudCommand
        : IRequest<GuardarEmpresaSolicitudResponse>
    {
        public GuardarEmpresaSolicitudRequest Request
        { get; }

        public string IdUsuarioAlta
        { get; }

        public GuardarEmpresaSolicitudCommand(
            GuardarEmpresaSolicitudRequest request,
            string idUsuarioAlta
        )
        {
            Request = request;
            IdUsuarioAlta = idUsuarioAlta;
        }
    }
}