using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;


namespace Auth
{
    public class JWT
    {

        public static SecurityToken GetAuthorizationToken(string authorizationSecret, AuthenticationIdentity identidad, string roles, DepartamentoSector deptoSector)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, identidad.name));
            claims.Add(new Claim(ClaimTypes.GivenName, identidad.nombre));
            claims.Add(new Claim(ClaimTypes.Surname, identidad.apellido));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, identidad.id));
            claims.Add(new Claim(ClaimTypes.Email, identidad.email));
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, identidad.authenticationMethod));
            claims.Add(new Claim(ClaimTypes.PrimaryGroupSid, identidad.path));
            claims.Add(new Claim(ClaimTypes.UserData, deptoSector.sector + "," + deptoSector.departamento));

            foreach (Rol rol in Rol.FromStringRepresentation(roles))
            {
                claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
            }

            ClaimsIdentity subject = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddMinutes(480),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authorizationSecret)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
        public static AuthenticationIdentity Validar(string token, string authenticationSecret)
        {

            var validationParameters = new TokenValidationParameters();
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authenticationSecret));
            validationParameters.IssuerSigningKey = signingKey;
            validationParameters.ValidateAudience = false;
            validationParameters.ValidateIssuer = false;
            var handler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;
            try
            {
                IdentityModelEventSource.ShowPII = true;
                principal = handler.ValidateToken(token, validationParameters, out securityToken);

            }
            catch (Exception ex)
            {
                throw new Exception("No autorizado");
            }

            return GetAuthenticationIdentity(principal.Identities.First().Claims);
        }

        private static AuthenticationIdentity GetAuthenticationIdentity(IEnumerable<Claim> claims)
        {
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var authenticationMethod = claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod);
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var apellido = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            var nombre = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            var id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var path = claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimaryGroupSid);

            var retorno = new AuthenticationIdentity(name.Value, authenticationMethod.Value, email.Value, apellido.Value, nombre.Value, id.Value, path.Value);

            return retorno;
        }
        public static AuthorizationIdentity GetAuthorizationIdentity(IEnumerable<Claim> claims)
        {
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var authenticationMethod = claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod);
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var apellido = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            var nombre = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            var id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var path = claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimaryGroupSid);
            var userData = claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);

            List<Rol> roles = new List<Rol>();

            foreach (Claim claim in claims.Where(c => c.Type == ClaimTypes.Role))
            {
                roles.Add(Rol.FromName(claim.Value));
            }

            var retorno = new AuthorizationIdentity(name.Value, authenticationMethod.Value, email.Value, apellido.Value, nombre.Value, id.Value, roles, userData.Value.Split(",")[0], userData.Value.Split(",")[1]);

            return retorno;
        }
        public class AuthorizationIdentity
        {
            public AuthorizationIdentity(string name, string authenticationMethod, string email, string apellido, string nombre, string id, List<Rol> roles, string sector, string departamento)
            {
                this.name = name;
                this.authenticationMethod = authenticationMethod;
                this.email = email;
                this.apellido = apellido;
                this.nombre = nombre;
                this.id = id;
                this.roles = roles;
                this.departamento = departamento;
                this.sector = sector;
            }

            public string name { get; set; }
            public string authenticationMethod { get; set; }
            public string email { get; set; }
            public string apellido { get; set; }
            public string nombre { get; set; }
            public string id { get; set; }
            public List<Rol> roles { get; set; }
            public string departamento { get; set; }
            public string sector { get; set; }
        }

        public class AuthenticationIdentity
        {
            public AuthenticationIdentity(string name, string authenticationMethod, string email, string apellido, string nombre, string id, string path)
            {
                this.name = name;
                this.authenticationMethod = authenticationMethod;
                this.email = email;
                this.apellido = apellido;
                this.nombre = nombre;
                this.id = id;
                this.path = path;

            }

            public string name { get; set; }
            public string authenticationMethod { get; set; }
            public string email { get; set; }
            public string apellido { get; set; }
            public string nombre { get; set; }
            public string id { get; set; }
            public string path { get; set; }

        }

        public static DepartamentoSector GetDepartamentoSector(string path, Usuario usuario)
        {
            // LDAP://CN=MDominguez,OU=Users,OU=Desarrollo,OU=Sistemas,OU=Ospecon,DC=rsuocra,DC=local
            // Para obtener Sector y Departamento descompongo el path en su partes.
            // el departamento y sector se encuentra en la cuarta y quinta posicion empezando de atras.
            var deptoSect = new DepartamentoSector();
            if (usuario == null || !usuario.Activo)
            {



                const int DEPARTAMENTO = 3;
                const int SECTOR = 4;

                var pathArr = path.Split(",");

                var cont = pathArr.Length;



                foreach (var el in pathArr.Reverse())
                {

                    if (cont == DEPARTAMENTO) deptoSect.departamento = el.Replace("OU=", "");
                    if (cont == SECTOR) deptoSect.sector = el.Replace("OU=", "");
                    cont--;
                }
            }
            else
            {
                deptoSect.sector = "CENTRAL";
                deptoSect.departamento = usuario.Area;
            }

            return deptoSect;
        }
        public class DepartamentoSector
        {
            public string departamento { get; set; }
            public string sector { get; set; }
        }
    }
}