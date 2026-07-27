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
    public class ActivarEmpresaCommand : IRequest<Guid>
    {

        [DataMember]
        public Guid EmpresaId { get; set; }


        public ActivarEmpresaCommand() { }
        public ActivarEmpresaCommand(Guid empresaId)

        {
            EmpresaId = empresaId;
        }
    }
}