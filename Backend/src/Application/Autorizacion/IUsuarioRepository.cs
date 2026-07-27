namespace Auth
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    public interface IUsuarioRepository
    {
        Task<IEnumerable<UsuarioDTO>> GetAllAsync();
        Task<bool> RegistrarContacto(Guid id, string email, string contacto, string usuarioUpdate);
        Task<Usuario> GetById(Guid id);
    }
}