using System;

namespace IERIC.SumariosIERIC.Services.CursosService.Domain.SeedWork
{
    public interface ITrack
    {
        Guid Id { get; set; }
        bool Activo { get; set; }
        DateTime FechaAlta { get; set; }
        string UsuarioAlta { get; set; }
        DateTime FechaUpdate { get; set; }
        string UsuarioUpdate { get; set; }

    }
}
