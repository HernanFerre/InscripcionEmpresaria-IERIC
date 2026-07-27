using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;
using IERIC.SumariosIERIC.Domain.Events;


namespace IERIC.SumariosIERIC.Domain.Entities
{
    public class Empresa : Entity, IAggregateRoot
    {

        public string RazonSocial { get; private set; }
        public Cuit Cuit { get; private set; }
        public bool EsCooperativa { get; private set; }
        public bool EstadoActivo { get; private set; }

        private Empresa() { }
        public Empresa(Cuit cuit, string razonSocial, bool esCooperativa)
        {
            Cuit = cuit;
            RazonSocial = razonSocial;
            EsCooperativa = esCooperativa;
            EstadoActivo = false;
            this.AddDomainEvent(new EmpresaNuevaRequested(this));
        }

        public bool ActivarEmpresa()
        {
            EstadoActivo = true;
            return EstadoActivo;
        }

    }
}