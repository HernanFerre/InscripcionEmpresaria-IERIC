using System;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Quiz.Mappers;
using IERIC.SumariosIERIC.Application.Quiz.Models;
using IERIC.SumariosIERIC.Application.Quiz.Settings;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.Services;
using MediatR;
using Microsoft.Extensions.Options;
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
        private readonly QuizSettings _quizSettings;

        public ValidarQuizCommandHandler(
            IQuizRepository quizRepository,
            IGeneradorQuiz generadorQuiz,
            IOptions<QuizSettings> quizSettings
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

            _quizSettings =
                quizSettings?.Value ??
                throw new ArgumentNullException(
                    nameof(quizSettings)
                );
        }

        public async Task<ValidarQuizResponse> Handle(
            ValidarQuizCommand command,
            CancellationToken cancellationToken
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    command.UsuarioId
                )
            )
            {
                throw new ForbiddenException(
                    "No fue posible identificar al usuario autenticado."
                );
            }

            QuizDominio quiz =
                await _quizRepository.ObtenerPorIdAsync(
                    command.QuizId
                );

            if (quiz == null)
            {
                throw new NotFoundException();
            }

            bool perteneceAlUsuario =
                string.Equals(
                    quiz.UsuarioId,
                    command.UsuarioId.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

            if (!perteneceAlUsuario)
            {
                throw new ForbiddenException(
                    "El quiz no pertenece al usuario autenticado."
                );
            }

            (
                bool estaBloqueado,
                DateTime? bloqueadoHasta
            ) =
                await _quizRepository
                    .ObtenerBloqueoVigenteAsync(
                        quiz.CuitEmpresa
                    );

            if (estaBloqueado)
            {
                string mensajeBloqueo =
                    bloqueadoHasta.HasValue
                        ? "El CUIT se encuentra bloqueado " +
                          "hasta " +
                          bloqueadoHasta.Value.ToString(
                              "dd/MM/yyyy HH:mm:ss"
                          ) +
                          "."
                        : "El CUIT se encuentra bloqueado.";

                throw new SumariosDomainException(
                    mensajeBloqueo
                );
            }

            quiz.MarcarComoExpirado();

            if (quiz.EstaExpirado)
            {
                await _quizRepository.GuardarAsync(
                    quiz
                );

                throw new SumariosDomainException(
                    "El quiz ha expirado"
                );
            }

            if (quiz.Estado == EstadoQuiz.Validado)
            {
                return new ValidarQuizResponse
                {
                    Ok = true,
                    LimiteExcedido = false,
                    IntentosRestantes =
                        quiz.IntentosRestantes,
                    BloqueadoHasta = null,
                    Mensaje =
                        "Información validada correctamente.",
                    NuevoQuiz = null
                };
            }

            TimeSpan? duracionBloqueo =
                _quizSettings
                    .ObtenerDuracionBloqueo();

            bool respuestaCorrecta =
                quiz.ValidarRespuesta(
                    command.OpcionesSeleccionadas,
                    duracionBloqueo
                );

            if (respuestaCorrecta)
            {
                await _quizRepository
                    .GuardarValidacionAsync(
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
                    BloqueadoHasta = null,
                    Mensaje =
                        "Información validada correctamente.",
                    NuevoQuiz = null
                };
            }

            if (quiz.LimiteExcedido)
            {
                await _quizRepository
                    .GuardarValidacionAsync(
                        quiz,
                        command.OpcionesSeleccionadas,
                        false
                    );

                string mensaje =
                    quiz.BloqueadoHasta.HasValue
                        ? "Se alcanzó el límite de intentos. " +
                          "El CUIT permanecerá bloqueado hasta " +
                          quiz.BloqueadoHasta.Value.ToString(
                              "dd/MM/yyyy HH:mm:ss"
                          ) +
                          "."
                        : "Se alcanzó el límite de intentos. " +
                          "El CUIT quedó bloqueado.";

                return new ValidarQuizResponse
                {
                    Ok = false,
                    LimiteExcedido = true,
                    IntentosRestantes = 0,
                    BloqueadoHasta =
                        quiz.BloqueadoHasta,
                    Mensaje = mensaje,
                    NuevoQuiz = null
                };
            }

            _generadorQuiz.GenerarNuevoDesafio(
                quiz
            );

            await _quizRepository
                .GuardarValidacionAsync(
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
                BloqueadoHasta = null,
                Mensaje =
                    "La respuesta no es correcta.",
                NuevoQuiz =
                    QuizResponseMapper.DesdeDominio(
                        quiz
                    )
            };
        }
    }
}