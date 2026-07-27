using MediatR;
using IERIC.SumariosIERIC.Domain.Entities;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace IERIC.SumariosIERIC.Application.Commands
{
    [DataContract]
    public class DeleteEmpresaCommand : IRequest<Guid>
    {
        [DataMember]
        public Guid EmpresaId { get; set; }

        public DeleteEmpresaCommand() { }
        public DeleteEmpresaCommand(Guid empresaId)
        {
            EmpresaId = empresaId;
        }
    }
}