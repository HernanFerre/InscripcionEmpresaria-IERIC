using System;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public class Empresa : IAggregateRoot
    {
        public long Id { get; private set; }

        public Cuit Cuit { get; private set; }

        public string RazonSocial { get; private set; }

        public int TipoSociedadId { get; private set; }

        public int? ActividadId { get; private set; }

        public int? CaracterId { get; private set; }

        public string Calle { get; private set; }

        public string Numero { get; private set; }

        public byte? Piso { get; private set; }

        public string DepartamentoOficina { get; private set; }

        public string CodigoPostal { get; private set; }

        public string Provincia { get; private set; }

        public string Localidad { get; private set; }

        public string Correo { get; private set; }

        public string Telefono { get; private set; }

        public bool TieneDatosCompletos =>
            ActividadId.HasValue &&
            CaracterId.HasValue &&
            !string.IsNullOrWhiteSpace(Calle) &&
            !string.IsNullOrWhiteSpace(Numero) &&
            !string.IsNullOrWhiteSpace(CodigoPostal) &&
            !string.IsNullOrWhiteSpace(Provincia) &&
            !string.IsNullOrWhiteSpace(Localidad) &&
            !string.IsNullOrWhiteSpace(Correo);

        private Empresa()
        {
        }

        private Empresa(
            Cuit cuit,
            string razonSocial,
            int tipoSociedadId
        )
        {
            Cuit = cuit ?? throw new SumariosDomainException(
                "La empresa debe tener un CUIT."
            );

            RazonSocial = NormalizarObligatorio(
                razonSocial,
                "razón social",
                150
            );

            ValidarIdentificadorCatalogo(
                tipoSociedadId,
                "tipo de sociedad"
            );

            TipoSociedadId = tipoSociedadId;
        }

        public static Empresa CrearIntegrante(
            Cuit cuit,
            string razonSocial,
            int tipoSociedadId
        )
        {
            return new Empresa(
                cuit,
                razonSocial,
                tipoSociedadId
            );
        }

        public static Empresa CrearPrincipal(
            Cuit cuit,
            string razonSocial,
            int tipoSociedadId,
            int actividadId,
            int caracterId,
            string calle,
            string numero,
            byte? piso,
            string departamentoOficina,
            string codigoPostal,
            string provincia,
            string localidad,
            string correo,
            string telefono
        )
        {
            Empresa empresa = new Empresa(
                cuit,
                razonSocial,
                tipoSociedadId
            );

            empresa.CompletarDatosInscripcion(
                actividadId,
                caracterId,
                calle,
                numero,
                piso,
                departamentoOficina,
                codigoPostal,
                provincia,
                localidad,
                correo,
                telefono
            );

            return empresa;
        }

        public void CompletarDatosInscripcion(
            int actividadId,
            int caracterId,
            string calle,
            string numero,
            byte? piso,
            string departamentoOficina,
            string codigoPostal,
            string provincia,
            string localidad,
            string correo,
            string telefono
        )
        {
            ValidarIdentificadorCatalogo(
                actividadId,
                "actividad"
            );

            ValidarIdentificadorCatalogo(
                caracterId,
                "carácter de la empresa"
            );

            string correoNormalizado = NormalizarObligatorio(
                correo,
                "correo electrónico",
                254
            );

            try
            {
                Email.FromAddress(correoNormalizado);
            }
            catch (ArgumentException)
            {
                throw new SumariosDomainException(
                    "El correo electrónico de la empresa no es válido."
                );
            }

            ActividadId = actividadId;
            CaracterId = caracterId;
            Calle = NormalizarObligatorio(calle, "calle", 150);
            Numero = NormalizarObligatorio(numero, "número", 20);
            Piso = piso;

            DepartamentoOficina = NormalizarOpcional(
                departamentoOficina,
                "departamento u oficina",
                20
            );

            CodigoPostal = NormalizarObligatorio(
                codigoPostal,
                "código postal",
                10
            );

            Provincia = NormalizarObligatorio(
                provincia,
                "provincia",
                100
            );

            Localidad = NormalizarObligatorio(
                localidad,
                "localidad",
                150
            );

            Correo = correoNormalizado;

            Telefono = NormalizarOpcional(
                telefono,
                "teléfono",
                30
            );
        }

        public void AsignarId(long id)
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador de la empresa no es válido."
                );
            }

            if (Id != 0)
            {
                throw new SumariosDomainException(
                    "La empresa ya tiene un identificador asignado."
                );
            }

            Id = id;
        }

        private static void ValidarIdentificadorCatalogo(
            int identificador,
            string nombreCampo
        )
        {
            if (identificador <= 0)
            {
                throw new SumariosDomainException(
                    $"El identificador de {nombreCampo} no es válido."
                );
            }
        }

        private static string NormalizarObligatorio(
            string valor,
            string nombreCampo,
            int longitudMaxima
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new SumariosDomainException(
                    $"El campo {nombreCampo} es obligatorio."
                );
            }

            string valorNormalizado = valor.Trim();

            if (valorNormalizado.Length > longitudMaxima)
            {
                throw new SumariosDomainException(
                    $"El campo {nombreCampo} supera la longitud permitida."
                );
            }

            return valorNormalizado;
        }

        private static string NormalizarOpcional(
            string valor,
            string nombreCampo,
            int longitudMaxima
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            string valorNormalizado = valor.Trim();

            if (valorNormalizado.Length > longitudMaxima)
            {
                throw new SumariosDomainException(
                    $"El campo {nombreCampo} supera " +
                    "la longitud permitida."
                );
            }

            return valorNormalizado;
        }
    }
}