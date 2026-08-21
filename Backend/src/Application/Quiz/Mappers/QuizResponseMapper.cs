using System.Linq;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using QuizDominio =
    IERIC.SumariosIERIC.Domain.Entities.Quiz;

namespace IERIC.SumariosIERIC.Application.Quiz.Mappers
{
    public static class QuizResponseMapper
    {
        public static QuizResponse DesdeDominio(
            QuizDominio quiz
        )
        {
            QuizResponse response =
                new QuizResponse
                {
                    QuizId = quiz.Id,
                    Titulo =
                        "INFORMACIÓN DE LA EMPRESA",
                    Consigna =
                        "Seleccione el o los CUIL que reconoce " +
                        "como vinculados a la empresa.",
                    IntentosTotales =
                        quiz.IntentosTotales,
                    IntentosRestantes =
                        quiz.IntentosRestantes,
                    Opciones =
                        quiz.Opciones
                            .Select(
                                opcion =>
                                    new QuizOpcionDto
                                    {
                                        Id =
                                            opcion.Id,
                                        Label =
                                            opcion.Id
                                                .ToUpperInvariant() +
                                            " - " +
                                            opcion
                                                .ObtenerCuilEnmascarado()
                                    }
                            )
                            .ToList()
                };

            response.Opciones.Add(
                new QuizOpcionDto
                {
                    Id = "ninguna",
                    Label =
                        "Ninguna de las anteriores"
                }
            );

            response.Opciones.Add(
                new QuizOpcionDto
                {
                    Id = "todas",
                    Label =
                        "Todas las anteriores"
                }
            );

            return response;
        }
    }
}