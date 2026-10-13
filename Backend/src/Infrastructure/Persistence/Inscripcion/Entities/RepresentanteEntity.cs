namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion
{
    public class RepresentanteEntity
    {
        public long Id { get; set; }

        public long IdEmpresa { get; set; }

        public string Apellido { get; set; }

        public string Nombre { get; set; }

        public long Cuil { get; set; }

        public int IdTipoRepresentante { get; set; }

        public string Correo { get; set; }

        public string Telefono { get; set; }
    }
}