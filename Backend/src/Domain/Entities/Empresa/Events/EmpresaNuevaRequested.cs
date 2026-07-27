using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
namespace IERIC.SumariosIERIC.Domain.Events
{
    public class EmpresaNuevaRequested : INotification
    {
        public Empresa Empresa { get; private set; }
        public EmpresaNuevaRequested(Empresa empresa)
        {
            Empresa = empresa;
        }
    }
}
