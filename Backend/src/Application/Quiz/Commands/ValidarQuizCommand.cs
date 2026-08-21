using System.Collections.Generic;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using MediatR;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class ValidarQuizCommand
        : IRequest<ValidarQuizResponse>
    {
        public long QuizId { get; }

        public string UsuarioId { get; }

        public List<string> OpcionesSeleccionadas
        { get; }

        public ValidarQuizCommand(
            long quizId,
            string usuarioId,
            IEnumerable<string> opcionesSeleccionadas
        )
        {
            QuizId = quizId;
            UsuarioId = usuarioId;

            OpcionesSeleccionadas =
                opcionesSeleccionadas == null
                    ? new List<string>()
                    : new List<string>(
                        opcionesSeleccionadas
                    );
        }
    }
}