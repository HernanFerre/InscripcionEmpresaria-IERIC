using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using IERIC.SumariosIERIC.Domain.Enums;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Application.Commands
{
    [DataContract]
    public class AddEmpresaCommand : IRequest<Guid>
    {

        [DataMember]
        public Int64 CUIT { get; set; }
        [DataMember]
        public string RazonSocial { get; set; }
        [DataMember]
        public bool EsCooperativa { get; set; }

        public AddEmpresaCommand() { }
        public AddEmpresaCommand(Int64 cuit, string razonSocial, bool esCooperativa)

        {
            CUIT = cuit;
            RazonSocial = razonSocial;
            EsCooperativa = esCooperativa;
        }
    }
}