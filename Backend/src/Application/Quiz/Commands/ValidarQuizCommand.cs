using System;
using System.Collections.Generic;
using MediatR;
using IERIC.SumariosIERIC.Application.Quiz.Models;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class ValidarQuizCommand
        : IRequest<ValidarQuizResponse>
    {
        public Guid QuizId { get; }

        public List<string> OpcionesSeleccionadas { get; }

        public ValidarQuizCommand(
            Guid quizId,
            IEnumerable<string> opcionesSeleccionadas
        )
        {
            QuizId = quizId;

            OpcionesSeleccionadas =
                opcionesSeleccionadas == null
                    ? new List<string>()
                    : new List<string>(
                        opcionesSeleccionadas
                    );
        }
    }
}