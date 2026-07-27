namespace Auth
{
    using Dapper;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using System.IdentityModel.Tokens.Jwt;
    using System.Diagnostics.Contracts;
    using Microsoft.AspNetCore.DataProtection;
    using System.Configuration;
    using System.Net;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AutorizacionRepository
        : IAutorizacionRepository
    {
        private AuthSettings _authSettings;
        public AutorizacionRepository(AuthSettings authSettings)
        {
            _authSettings = authSettings;
        }
        public async Task<AutorizacionDTO> AutorizarAsync(string token)
        {

            JWT.AuthenticationIdentity identidad = JWT.Validar(token, _authSettings.AuthenticationSecret);

            Usuario? usuario = null;

            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {
                string query = "select * from usuario where Identificacion = @id";

                usuario = connection.QueryFirstOrDefault<Usuario>(query, new { identidad.id });
            }

            JWT.DepartamentoSector deptoSec = JWT.GetDepartamentoSector(identidad.path, usuario);

            AutorizacionDTO retorno = new AutorizacionDTO
            {
                Apellido = identidad.apellido,
                Nombre = identidad.nombre,
                NombreUsuario = identidad.name,
                Departamento = deptoSec.departamento,
                Sector = deptoSec.sector,
                Email = identidad.email,
                Roles = Rol.FromStringRepresentation(Rol.Usuario.Nombre)
            };

            if (usuario != null && usuario.Activo) retorno.Roles = Rol.FromStringRepresentation(usuario.Roles);

            var tokenHandler = new JwtSecurityTokenHandler();
            var authToken = JWT.GetAuthorizationToken(_authSettings.AuthorizationSecret, identidad, Rol.ToStringRepresentation(retorno.Roles), deptoSec);
            var newStringToken = tokenHandler.WriteToken(authToken);
            retorno.Token = newStringToken;

            return retorno;

        }

        public async Task<Guid> RegistrarUsuarioAsync(string token)
        {

            JWT.AuthenticationIdentity identidad = JWT.Validar(token, _authSettings.AuthenticationSecret);


            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {

                string query = "select * from usuario where Identificacion = @id";

                IEnumerable<Usuario> usuarios = await connection.QueryAsync<Usuario>(query, new { identidad.id });

                if (usuarios.Any()) throw new Exception("El usuario ya se encuentra registrado");

                string sql =
                "INSERT INTO Usuario ([Id],[NombreUsuario],[Apellido],[Nombre],[Email],[Identificacion],[Activo],[FechaAlta],[UsuarioAlta],[Roles])" +
                " VALUES (@Id,@NombreUsuario,@Apellido,@Nombre,@Email,@Identificacion,@Activo,@FechaAlta,@UsuarioAlta,@Roles)";

                var newId = Guid.NewGuid();
                var nuevoUsuario = new
                {
                    Id = newId.ToString(),
                    NombreUsuario = identidad.name,
                    Apellido = identidad.apellido,
                    Nombre = identidad.nombre,
                    Email = identidad.email,
                    Identificacion = identidad.id,
                    Activo = false,
                    FechaAlta = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    UsuarioAlta = identidad.name,
                    Roles = "Sin_Permisos",
                };

                var resultado = await connection.ExecuteAsync(sql, nuevoUsuario);
                if (resultado == 0) throw new Exception("No se pudo registrar el usuario");
                return newId;

            }
        }

        public async Task<bool> ActivarUsuarioAsync(Guid id, List<Rol> roles, string area)
        {

            using (var connection = new SqlConnection(_authSettings.ConnectionString))
            {

                string query = "select * from usuario where Identificacion = @id";

                IEnumerable<Usuario> usuarios = await connection.QueryAsync<Usuario>(query, new { id });

                if (!usuarios.Any()) throw new Exception("El usuario no se encuentra");

                string sql = "update Usuario set Activo = 1, Roles = @roles, Area = @area where Identificacion = @id";

                var resultado = await connection.ExecuteAsync(sql, new { roles = Rol.ToStringRepresentation(roles), id = id.ToString(), area = area });

                if (resultado == 0) throw new Exception("No se pudo registrar el usuario");

                return resultado == 1;

            }
        }
    }

}