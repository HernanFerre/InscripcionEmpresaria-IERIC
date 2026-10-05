using IERIC.SumariosIERIC.Application.Inscripcion.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class GuardarEmpresaSolicitudCommand
        : IRequest<GuardarEmpresaSolicitudResponse>
    {
        public GuardarEmpresaSolicitudRequest Request
        { get; }

        public string UsuarioId
        { get; }

        public GuardarEmpresaSolicitudCommand(
            GuardarEmpresaSolicitudRequest request,
            string usuarioId
        )
        {
            Request = request;
            UsuarioId = usuarioId;
        }
    }
}