using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Quiz.Mappers;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.Services;
using QuizDominio =
    IERIC.SumariosIERIC.Domain.Entities.Quiz;

namespace IERIC.SumariosIERIC.Application.Commands
{
    public class ValidarQuizCommandHandler
        : IRequestHandler<
            ValidarQuizCommand,
            ValidarQuizResponse
        >
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IGeneradorQuiz _generadorQuiz;

        public ValidarQuizCommandHandler(
            IQuizRepository quizRepository,
            IGeneradorQuiz generadorQuiz
        )
        {
            _quizRepository =
                quizRepository ??
                throw new ArgumentNullException(
                    nameof(quizRepository)
                );

            _generadorQuiz =
                generadorQuiz ??
                throw new ArgumentNullException(
                    nameof(generadorQuiz)
                );
        }

        public async Task<ValidarQuizResponse> Handle(
            ValidarQuizCommand command,
            CancellationToken cancellationToken
        )
        {
            QuizDominio quiz =
                await _quizRepository.ObtenerPorIdAsync(
                    command.QuizId
                );

            if (quiz == null)
            {
                throw new NotFoundException();
            }

            quiz.MarcarComoExpirado();

            if (quiz.EstaExpirado)
            {
                await _quizRepository.GuardarAsync(quiz);

                throw new SumariosDomainException(
                    "El quiz ha expirado"
                );
            }

            bool respuestaCorrecta =
                quiz.ValidarRespuesta(
                    command.OpcionesSeleccionadas
                );

            if (respuestaCorrecta)
            {
                await _quizRepository.GuardarValidacionAsync(
                    quiz,
                    command.OpcionesSeleccionadas,
                    true
                );

                return new ValidarQuizResponse
                {
                    Ok = true,
                    LimiteExcedido = false,
                    IntentosRestantes =
                        quiz.IntentosRestantes,
                    Mensaje =
                        "Información validada correctamente.",
                    NuevoQuiz = null
                };
            }

            if (quiz.LimiteExcedido)
            {
                await _quizRepository.GuardarValidacionAsync(
                    quiz,
                    command.OpcionesSeleccionadas,
                    false
                );

                return new ValidarQuizResponse
                {
                    Ok = false,
                    LimiteExcedido = true,
                    IntentosRestantes = 0,
                    Mensaje =
                        "Se alcanzó el límite de intentos.",
                    NuevoQuiz = null
                };
            }

            _generadorQuiz.GenerarNuevoDesafio(quiz);

            await _quizRepository.GuardarValidacionAsync(
                quiz,
                command.OpcionesSeleccionadas,
                false
            );

            return new ValidarQuizResponse
            {
                Ok = false,
                LimiteExcedido = false,
                IntentosRestantes =
                    quiz.IntentosRestantes,
                Mensaje =
                    "La respuesta no es correcta.",
                NuevoQuiz =
                    QuizResponseMapper.DesdeDominio(quiz)
            };
        }
    }
}