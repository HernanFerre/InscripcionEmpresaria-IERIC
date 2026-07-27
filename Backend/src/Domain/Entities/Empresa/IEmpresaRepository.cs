using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities
{
    public interface IEmpresaRepository : IRepository<Empresa>
    {
        Empresa Add(Empresa Empresa);
        Task<Empresa> GetById(Guid id);
        Task<Empresa> GetByCuit(Int64 cuil);
        Empresa Delete(Empresa Empresa);
        bool ActivarEmpresa(Empresa Empresa);
        Task<bool> ExistAny(Cuit cuit);

    }
}