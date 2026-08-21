using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public interface IQuizRepository
    {
        Task GuardarAsync(Quiz quiz);

        Task<Quiz> ObtenerPorIdAsync(long quizId);

        Task GuardarValidacionAsync(
            Quiz quiz,
            IEnumerable<string> opcionesSeleccionadas,
            bool respuestaCorrecta
        );

        Task<(
            bool EstaBloqueado,
            DateTime? BloqueadoHasta
        )> ObtenerBloqueoVigenteAsync(
            Cuit cuitEmpresa
        );
    }
}