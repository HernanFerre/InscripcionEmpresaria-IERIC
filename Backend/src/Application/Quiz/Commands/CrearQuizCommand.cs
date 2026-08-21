using IERIC.SumariosIERIC.Application.Quiz.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CrearQuizCommand
        : IRequest<QuizResponse>
    {
        public string Cuit { get; }

        public string UsuarioId { get; }

        public CrearQuizCommand(
            string cuit,
            string usuarioId
        )
        {
            Cuit = cuit;
            UsuarioId = usuarioId;
        }
    }
}