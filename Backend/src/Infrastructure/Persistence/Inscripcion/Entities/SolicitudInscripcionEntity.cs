using System;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion
{
    public class SolicitudInscripcionEntity
    {
        public long Id { get; set; }

        public long Idempresa { get; set; }

        public Guid UsuarioId { get; set; }

        public string Comentarios { get; set; }

        public string Ua { get; set; }

        public string Um { get; set; }

        public DateTime? Fa { get; set; }

        public DateTime? Fm { get; set; }
    }
}