using System;
using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public interface IQuizRepository
    {
        Task GuardarAsync(Quiz quiz);

        Task<Quiz> ObtenerPorIdAsync(Guid quizId);

        Task EliminarAsync(Guid quizId);
    }
}