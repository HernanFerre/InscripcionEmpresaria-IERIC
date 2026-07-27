using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;


namespace Auth
{
    public class AutorizacionDTO
    {

        public string NombreUsuario { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Token { get; set; }
        public List<Rol> Roles { get; set; }
        public string Email { get; set; }
        public string Contacto { get; set; }
        public string Departamento { get; set; }
        public string Sector { get; set; }

    }

}