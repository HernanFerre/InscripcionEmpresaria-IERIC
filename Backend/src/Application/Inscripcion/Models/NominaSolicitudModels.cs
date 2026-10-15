using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Application.Inscripcion.Models
{
    public class GuardarNominaSolicitudRequest
    {
        public List<TrabajadorNominaSolicitudRequest> Trabajadores
        { get; set; } = new List<TrabajadorNominaSolicitudRequest>();
    }

    public class TrabajadorNominaSolicitudRequest
    {
        public long TrabajadorId { get; set; }

        public string ApellidoYNombre { get; set; }

        public string Cuil { get; set; }

        public DateTime? Ingreso { get; set; }

        public DateTime? Egreso { get; set; }

        public bool? PerteneceAConstruccion { get; set; }
    }

    public class NominaSolicitudResponse
    {
        public long SolicitudId { get; set; }

        public int CantidadTrabajadores { get; set; }

        public DateTime? FechaInicioActividades { get; set; }

        public int? DiasDesdeInicioActividades { get; set; }

        public bool RequiereDeclaracionJurada { get; set; }

        public List<TrabajadorNominaSolicitudResponse> Trabajadores
        { get; set; } = new List<TrabajadorNominaSolicitudResponse>();
    }

    public class TrabajadorNominaSolicitudResponse
    {
        public long TrabajadorId { get; set; }

        public string ApellidoYNombre { get; set; }

        public string Cuil { get; set; }

        public DateTime Ingreso { get; set; }

        public DateTime? Egreso { get; set; }

        public bool PerteneceAConstruccion { get; set; }

        public bool EsManual { get; set; }
    }
}