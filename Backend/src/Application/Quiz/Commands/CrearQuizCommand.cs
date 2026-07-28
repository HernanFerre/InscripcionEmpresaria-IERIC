using System.Collections.Generic;
using MediatR;
using IERIC.SumariosIERIC.Application.Quiz.Models;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class CrearQuizCommand
        : IRequest<QuizResponse>
    {
        public string Cuit { get; }

        public List<string> Cuiles { get; }

        public CrearQuizCommand(
            string cuit,
            IEnumerable<string> cuiles
        )
        {
            Cuit = cuit;

            Cuiles = cuiles == null
                ? new List<string>()
                : new List<string>(cuiles);
        }
    }
}