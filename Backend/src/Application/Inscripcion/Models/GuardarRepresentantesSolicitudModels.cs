using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Application.Inscripcion.Models
{
    public class GuardarRepresentantesSolicitudRequest
    {
        public List<RepresentanteSolicitudRequest> Representantes
        { get; set; } =
            new List<RepresentanteSolicitudRequest>();
    }

    public class RepresentanteSolicitudRequest
    {
        public long EmpresaId { get; set; }

        public string Apellido { get; set; }

        public string Nombre { get; set; }

        public string Cuil { get; set; }

        public int TipoRepresentanteId { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }
    }

    public class GuardarRepresentantesSolicitudResponse
    {
        public long SolicitudId { get; set; }

        public int CantidadRepresentantes { get; set; }

        public List<RepresentanteGuardadoResponse> Representantes
        { get; set; } =
            new List<RepresentanteGuardadoResponse>();
    }

    public class RepresentanteGuardadoResponse
    {
        public long RepresentanteId { get; set; }

        public long EmpresaId { get; set; }

        public string Apellido { get; set; }

        public string Nombre { get; set; }

        public string Cuil { get; set; }

        public int TipoRepresentanteId { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }
    }
}