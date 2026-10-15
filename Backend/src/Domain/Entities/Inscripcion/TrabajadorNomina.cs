using System;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public class TrabajadorNomina
    {
        public long Id { get; private set; }

        public long SolicitudId { get; private set; }

        public string ApellidoYNombre { get; private set; }

        public Cuil Cuil { get; private set; }

        public DateTime Ingreso { get; private set; }

        public DateTime? Egreso { get; private set; }

        public bool PerteneceAConstruccion { get; private set; }

        public bool EsManual { get; private set; }

        private TrabajadorNomina()
        {
        }

        private TrabajadorNomina(
            long solicitudId,
            string apellidoYNombre,
            Cuil cuil,
            DateTime ingreso,
            DateTime? egreso,
            bool perteneceAConstruccion,
            bool esManual
        )
        {
            if (solicitudId <= 0)
            {
                throw new SumariosDomainException(
                    "La solicitud del trabajador no es válida."
                );
            }

            AsignarDatos(
                apellidoYNombre,
                cuil,
                ingreso,
                egreso
            );

            SolicitudId = solicitudId;
            PerteneceAConstruccion = perteneceAConstruccion;
            EsManual = esManual;
        }

        public static TrabajadorNomina CrearDesdeServicio(
            long solicitudId,
            string apellidoYNombre,
            Cuil cuil,
            DateTime ingreso,
            DateTime? egreso = null,
            bool perteneceAConstruccion = true
        )
        {
            return new TrabajadorNomina(
                solicitudId,
                apellidoYNombre,
                cuil,
                ingreso,
                egreso,
                perteneceAConstruccion,
                false
            );
        }

        public static TrabajadorNomina CrearManual(
            long solicitudId,
            string apellidoYNombre,
            Cuil cuil,
            DateTime ingreso,
            DateTime? egreso = null,
            bool perteneceAConstruccion = true
        )
        {
            return new TrabajadorNomina(
                solicitudId,
                apellidoYNombre,
                cuil,
                ingreso,
                egreso,
                perteneceAConstruccion,
                true
            );
        }

        public void ActualizarDatosManuales(
            string apellidoYNombre,
            Cuil cuil,
            DateTime ingreso,
            DateTime? egreso
        )
        {
            if (!EsManual)
            {
                throw new SumariosDomainException(
                    "Los datos de los trabajadores provenientes del servicio " +
                    "no pueden modificarse."
                );
            }

            AsignarDatos(
                apellidoYNombre,
                cuil,
                ingreso,
                egreso
            );
        }

        public void ActualizarPertenenciaConstruccion(
            bool perteneceAConstruccion
        )
        {
            PerteneceAConstruccion = perteneceAConstruccion;
        }

        public void AsignarId(long id)
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador del trabajador no es válido."
                );
            }

            if (Id != 0)
            {
                throw new SumariosDomainException(
                    "El trabajador ya tiene un identificador asignado."
                );
            }

            Id = id;
        }

        private void AsignarDatos(
            string apellidoYNombre,
            Cuil cuil,
            DateTime ingreso,
            DateTime? egreso
        )
        {
            if (string.IsNullOrWhiteSpace(apellidoYNombre))
            {
                throw new SumariosDomainException(
                    "El apellido y nombre del trabajador es obligatorio."
                );
            }

            string nombreNormalizado = apellidoYNombre.Trim();

            if (nombreNormalizado.Length > 200)
            {
                throw new SumariosDomainException(
                    "El apellido y nombre del trabajador supera los 200 caracteres."
                );
            }

            if (cuil == null)
            {
                throw new SumariosDomainException(
                    "El trabajador debe tener un CUIL."
                );
            }

            if (ingreso == default(DateTime))
            {
                throw new SumariosDomainException(
                    "La fecha de ingreso del trabajador es obligatoria."
                );
            }

            DateTime ingresoNormalizado = ingreso.Date;
            DateTime? egresoNormalizado = egreso?.Date;

            if (egresoNormalizado.HasValue &&
                egresoNormalizado.Value < ingresoNormalizado)
            {
                throw new SumariosDomainException(
                    "La fecha de egreso no puede ser anterior a la fecha de ingreso."
                );
            }

            ApellidoYNombre = nombreNormalizado;
            Cuil = cuil;
            Ingreso = ingresoNormalizado;
            Egreso = egresoNormalizado;
        }
    }
}