using System.Threading.Tasks;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public interface ISolicitudInscripcionRepository
    {
        Task GuardarAsync(
            SolicitudInscripcion solicitud
        );
    }
}