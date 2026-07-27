namespace Auth
{
    using Dapper;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using System.IdentityModel.Tokens.Jwt;
    using System.Diagnostics.Contracts;
    using System.Collections.Generic;
    using System;
    using System.Linq;

    public class UsuarioRepository
        : IUsuarioRepository
    {
        private AuthSettings _authSettings;
        public UsuarioRepository(AuthSettings authSettings)
        {
            _authSettings = authSettings;
        }


        public async Task<IEnumerable<UsuarioDTO>> GetAllAsync()
        {


            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {

                string query = "select * from usuario";

                IEnumerable<UsuarioDTO> usuarios = await connection.QueryAsync<UsuarioDTO>(query);


                return usuarios;

            }
        }

        public async Task<Usuario> GetById(Guid id)
        {
            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {
                string query = "select * from usuario where id = @id";
                Usuario usuario = (await connection.QueryAsync<Usuario>(query, new { id })).FirstOrDefault();
                return usuario;
            }
        }

        public async Task<bool> RegistrarContacto(Guid id, string email, string contacto, string usuarioUpdate)
        {


            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {

                string sql = "update Usuario set Contacto = @Contacto, Email = @email, FechaUpdate = @fechaUpdate, UsuarioUpdate = @usuarioUpdate where Identificacion = @id";

                var resultado = await connection.ExecuteAsync(sql, new { id, email, contacto, fechaUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), usuarioUpdate = usuarioUpdate });

                if (resultado == 0) throw new Exception("No se pudo registrar el contacto en el usuario");

                return resultado == 1;

            }
        }

    }

}