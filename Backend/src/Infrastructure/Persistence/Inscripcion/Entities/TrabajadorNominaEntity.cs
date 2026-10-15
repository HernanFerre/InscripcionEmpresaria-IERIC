using System;

namespace IERIC.SumariosIERIC.Infrastructure.Persistence.Inscripcion
{
    public class TrabajadorNominaEntity
    {
        public long Id { get; set; }

        public long IdSolicitud { get; set; }

        public string ApellidoYNombre { get; set; }

        public string Cuil { get; set; }

        public DateTime Ingreso { get; set; }

        public DateTime? Egreso { get; set; }

        public bool PerteneceAConstr { get; set; }

        public bool EsManual { get; set; }
    }
}