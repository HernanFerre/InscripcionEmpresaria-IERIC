namespace Auth
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    public interface IAutorizacionRepository
    {
        Task<AutorizacionDTO> AutorizarAsync(string token);
        Task<Guid> RegistrarUsuarioAsync(string token);
        Task<bool> ActivarUsuarioAsync(Guid id, List<Rol> roles, string area);

    }
}