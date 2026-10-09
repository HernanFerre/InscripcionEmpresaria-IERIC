using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Application.Inscripcion.Models
{
    public class GuardarEmpresaSolicitudRequest
    {
        public string Cuit { get; set; }

        public string RazonSocial { get; set; }

        public int ActividadId { get; set; }

        public int CaracterId { get; set; }

        public int TipoSociedadId { get; set; }

        public string Calle { get; set; }

        public string Numero { get; set; }

        public byte? Piso { get; set; }

        public string DepartamentoOficina { get; set; }

        public string CodigoPostal { get; set; }

        public int? IdProvincia { get; set; }

        public int? IdLocalidad { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }

        public List<EmpresaIntegranteRequest>
            EmpresasIntegrantes
        { get; set; } =
            new List<EmpresaIntegranteRequest>();
    }

    public class EmpresaIntegranteRequest
    {
        public string Cuit { get; set; }

        public string RazonSocial { get; set; }

        public int TipoSociedadId { get; set; }
    }

    public class GuardarEmpresaSolicitudResponse
    {
        public long SolicitudId { get; set; }

        public long EmpresaId { get; set; }

        public string CuitEmpresa { get; set; }

        public int CantidadEmpresasIntegrantes { get; set; }

        public List<EmpresaIntegranteGuardadaResponse>
            EmpresasIntegrantes
        { get; set; } =
            new List<EmpresaIntegranteGuardadaResponse>();
    }

    public class EmpresaIntegranteGuardadaResponse
    {
        public long EmpresaId { get; set; }

        public string Cuit { get; set; }

        public string RazonSocial { get; set; }

        public int TipoSociedadId { get; set; }

        public int LegacyId { get; set; }
    }
}