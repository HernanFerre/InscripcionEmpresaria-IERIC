using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class QuizEnMemoriaRepository
        : IQuizRepository
    {
        private readonly ConcurrentDictionary<long, Quiz>
            _quizzes =
                new ConcurrentDictionary<long, Quiz>();

        private long _ultimoId;

        public Task GuardarAsync(
            Quiz quiz
        )
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(
                    nameof(quiz)
                );
            }

            if (quiz.Id == 0)
            {
                long nuevoId =
                    Interlocked.Increment(
                        ref _ultimoId
                    );

                quiz.AsignarId(
                    nuevoId
                );
            }

            _quizzes.AddOrUpdate(
                quiz.Id,
                quiz,
                (_, __) => quiz
            );

            return Task.CompletedTask;
        }

        public Task<Quiz> ObtenerPorIdAsync(
            long quizId
        )
        {
            _quizzes.TryGetValue(
                quizId,
                out Quiz quiz
            );

            return Task.FromResult(
                quiz
            );
        }

        public Task GuardarValidacionAsync(
            Quiz quiz,
            IEnumerable<string> opcionesSeleccionadas,
            bool respuestaCorrecta
        )
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(
                    nameof(quiz)
                );
            }

            return GuardarAsync(
                quiz
            );
        }

        public Task<(
            bool EstaBloqueado,
            DateTime? BloqueadoHasta
        )> ObtenerBloqueoVigenteAsync(
            Cuit cuitEmpresa
        )
        {
            if (cuitEmpresa == null)
            {
                throw new ArgumentNullException(
                    nameof(cuitEmpresa)
                );
            }

            long numeroCuit =
                cuitEmpresa.ToInt64();

            DateTime fechaActual =
                DateTime.Now;

            Quiz quizBloqueado =
                _quizzes.Values
                    .Where(
                        quiz =>
                            quiz.CuitEmpresa.ToInt64() ==
                                numeroCuit &&
                            quiz.Estado ==
                                EstadoQuiz.Bloqueado &&
                            (
                                !quiz.BloqueadoHasta.HasValue ||
                                quiz.BloqueadoHasta.Value >
                                    fechaActual
                            )
                    )
                    .OrderByDescending(
                        quiz => quiz.FechaCreacion
                    )
                    .FirstOrDefault();

            if (quizBloqueado == null)
            {
                return Task.FromResult(
                    (
                        EstaBloqueado: false,
                        BloqueadoHasta:
                            (DateTime?)null
                    )
                );
            }

            return Task.FromResult(
                (
                    EstaBloqueado: true,
                    BloqueadoHasta:
                        quizBloqueado.BloqueadoHasta
                )
            );
        }
    }
}