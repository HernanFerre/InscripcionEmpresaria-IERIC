using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public interface IQuizRepository
    {
        Task GuardarAsync(Quiz quiz);

        Task<Quiz> ObtenerPorIdAsync(Guid quizId);

        Task GuardarValidacionAsync(
            Quiz quiz,
            IEnumerable<string> opcionesSeleccionadas,
            bool respuestaCorrecta
        );
    }
}