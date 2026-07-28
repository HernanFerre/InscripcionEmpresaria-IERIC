namespace IERIC.SumariosIERIC.Domain.Services
{
    public interface IAleatoriedadQuiz
    {
        int ObtenerEntero(
            int minimoIncluido,
            int maximoExcluido
        );
    }
}