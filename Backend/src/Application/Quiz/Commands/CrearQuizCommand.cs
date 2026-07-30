using MediatR;
using IERIC.SumariosIERIC.Application.Quiz.Models;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CrearQuizCommand
        : IRequest<QuizResponse>
    {
        public string Cuit { get; }

        public CrearQuizCommand(
            string cuit
        )
        {
            Cuit = cuit;
        }
    }
}