using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using IERIC.SumariosIERIC.Domain.Entities;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class QuizEnMemoriaRepository
        : IQuizRepository
    {
        private readonly ConcurrentDictionary<Guid, Quiz> _quizzes =
            new ConcurrentDictionary<Guid, Quiz>();

        public Task GuardarAsync(Quiz quiz)
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(nameof(quiz));
            }

            _quizzes.AddOrUpdate(
                quiz.Id,
                quiz,
                (_, quizExistente) => quiz
            );

            return Task.CompletedTask;
        }

        public Task<Quiz> ObtenerPorIdAsync(Guid quizId)
        {
            _quizzes.TryGetValue(
                quizId,
                out Quiz quiz
            );

            return Task.FromResult(quiz);
        }

        public Task GuardarValidacionAsync(
            Quiz quiz,
            IEnumerable<string> opcionesSeleccionadas,
            bool respuestaCorrecta
        )
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(nameof(quiz));
            }

            return GuardarAsync(quiz);
        }
    }
}