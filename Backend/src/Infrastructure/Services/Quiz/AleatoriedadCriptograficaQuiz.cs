using System.Security.Cryptography;
using IERIC.SumariosIERIC.Domain.Services;

namespace IERIC.SumariosIERIC.Infrastructure.Services
{
    public class AleatoriedadCriptograficaQuiz
        : IAleatoriedadQuiz
    {
        public int ObtenerEntero(
            int minimoIncluido,
            int maximoExcluido
        )
        {
            return RandomNumberGenerator.GetInt32(
                minimoIncluido,
                maximoExcluido
            );
        }
    }
}