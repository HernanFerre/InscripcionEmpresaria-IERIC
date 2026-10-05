using System;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion
{
    public class EmpresaEntity
    {
        public long Id { get; set; }

        public string RazonSocial { get; set; }

        public long Cuit { get; set; }

        public bool EsCooperativa { get; set; }

        public bool EstadoActivo { get; set; }

        public int LegacyId { get; set; }

        public bool Activo { get; set; }

        public DateTime Fa { get; set; }

        public string Ua { get; set; }

        public DateTime Fm { get; set; }

        public string Um { get; set; }

        public string Calle { get; set; }

        public string Numero { get; set; }

        public byte? Piso { get; set; }

        public string DeptoOficina { get; set; }

        public string CodigoPostal { get; set; }

        public string Provincia { get; set; }

        public string Localidad { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }

        public int IdActividadsolicitud { get; set; }

        public int IdCaracter { get; set; }

        public int IdTipoSoc { get; set; }

        public bool DDJJ { get; set; }
    }
}