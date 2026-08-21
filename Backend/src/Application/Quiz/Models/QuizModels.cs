using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Application.Quiz.Models
{
    public class CrearQuizRequest
    {
        public string Cuit { get; set; }
    }

    public class QuizOpcionDto
    {
        public string Id { get; set; }

        public string Label { get; set; }
    }

    public class QuizResponse
    {
        public long QuizId { get; set; }

        public string Titulo { get; set; }

        public string Consigna { get; set; }

        public int IntentosTotales { get; set; }

        public int IntentosRestantes { get; set; }

        public List<QuizOpcionDto> Opciones { get; set; } =
            new List<QuizOpcionDto>();
    }

    public class ValidarQuizRequest
    {
        public long QuizId { get; set; }

        public List<string> OpcionesSeleccionadas
        { get; set; } =
            new List<string>();
    }

    public class ValidarQuizResponse
    {
        public bool Ok { get; set; }

        public bool LimiteExcedido { get; set; }

        public int IntentosRestantes { get; set; }

        public DateTime? BloqueadoHasta { get; set; }

        public string Mensaje { get; set; }

        public QuizResponse NuevoQuiz { get; set; }
    }
}