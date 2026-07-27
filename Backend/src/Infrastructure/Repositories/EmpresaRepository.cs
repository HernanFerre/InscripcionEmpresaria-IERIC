using Microsoft.EntityFrameworkCore;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.SeedWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Infrastructure.Repositories
{
    public class EmpresaRepository
        : IEmpresaRepository
    {
        private readonly SumariosContext _context;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _context;
            }
        }

        public EmpresaRepository(SumariosContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Empresa Add(Empresa Empresa)
        {
            return _context.Empresas.Add(Empresa).Entity;
        }

        public async Task<Empresa> GetById(Guid id)
        {
            var item = _context
                                .Empresas
                                .Local
                                .FirstOrDefault(o => o.Id == id);
            if (item == null)
            {
                item = await _context
                            .Empresas
                            .FirstOrDefaultAsync(o => o.Id == id);
            }

            return item;
        }

        public async Task<Empresa> GetByCuit(Int64 cuit)
        {

            var item = _context
                                 .Empresas
                                 .Local
                                 .FirstOrDefault(o => o.Cuit == cuit);
            if (item == null)
            {
                item = await _context
                            .Empresas
                            .FirstOrDefaultAsync(o => o.Cuit == cuit);
            }

            return item;
        }

        public Empresa Delete(Empresa Empresa)
        {
            // _context.Empresas.Remove(Empresa);
            Empresa.Activo = false;
            return Empresa;
        }

        public async Task<bool> ExistAny(Cuit cuit)
        {
            var retorno = await _context
                         .Empresas
                         .AnyAsync(o => o.Cuit == cuit);

            return retorno;
        }

        public bool ActivarEmpresa(Empresa Empresa)
        {
            Empresa.ActivarEmpresa();
            return Empresa.EstadoActivo;
        }
    }
}