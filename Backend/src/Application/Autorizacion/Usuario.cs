using System;

namespace Auth
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public Guid Identificacion { get; set; }
        public string NombreUsuario { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string Roles { get; set; }
        public string Email { get; set; }
        public string Contacto { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaUpdate { get; set; }
        public string UsuarioAlta { get; set; }
        public string UsuarioUpdate { get; set; }
        public string Area { get; set; }

    }
}