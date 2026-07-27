namespace IERIC.SumariosIERIC.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IERIC.SumariosIERIC.Domain.Entities;
    using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
    using static Auth.JWT;

    public interface IEmpresaQueries
    {
        Task<EmpresaDTO> GetById(Guid id);
        Task<IEnumerable<EmpresaDTO>> GetAll();
        Task<EmpresaDTO> GetByCuit(Int64 cuil);

    }
}