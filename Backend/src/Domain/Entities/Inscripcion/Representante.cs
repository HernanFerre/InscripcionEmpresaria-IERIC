using System;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.Entities.Inscripcion
{
    public class Representante
    {
        public long Id { get; private set; }

        public long EmpresaId { get; private set; }

        public string Apellido { get; private set; }

        public string Nombre { get; private set; }

        public Cuil Cuil { get; private set; }

        public int TipoRepresentanteId { get; private set; }

        public string Correo { get; private set; }

        public string Telefono { get; private set; }

        private Representante()
        {
        }

        private Representante(
            long empresaId,
            string apellido,
            string nombre,
            Cuil cuil,
            int tipoRepresentanteId,
            string correo,
            string telefono
        )
        {
            if (empresaId <= 0)
            {
                throw new SumariosDomainException(
                    "La empresa del representante no es válida."
                );
            }

            if (cuil == null)
            {
                throw new SumariosDomainException(
                    "El representante debe tener un CUIL."
                );
            }

            if (tipoRepresentanteId <= 0)
            {
                throw new SumariosDomainException(
                    "El tipo de representante no es válido."
                );
            }

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
                    "El correo electrónico del representante no es válido."
                );
            }

            ValidarTelefono(telefono);

            EmpresaId = empresaId;

            Apellido = NormalizarObligatorio(
                apellido,
                "apellido",
                100
            );

            Nombre = NormalizarObligatorio(
                nombre,
                "nombre",
                100
            );

            Cuil = cuil;
            TipoRepresentanteId = tipoRepresentanteId;
            Correo = correoNormalizado;
            Telefono = telefono.Trim();
        }

        public static Representante Crear(
            long empresaId,
            string apellido,
            string nombre,
            Cuil cuil,
            int tipoRepresentanteId,
            string correo,
            string telefono
        )
        {
            return new Representante(
                empresaId,
                apellido,
                nombre,
                cuil,
                tipoRepresentanteId,
                correo,
                telefono
            );
        }

        public void AsignarId(long id)
        {
            if (id <= 0)
            {
                throw new SumariosDomainException(
                    "El identificador del representante no es válido."
                );
            }

            if (Id != 0)
            {
                throw new SumariosDomainException(
                    "El representante ya tiene un identificador asignado."
                );
            }

            Id = id;
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

        private static void ValidarTelefono(string telefono)
        {
            string telefonoNormalizado = NormalizarObligatorio(
                telefono,
                "teléfono",
                30
            );

            int cantidadNumeros = 0;

            foreach (char caracter in telefonoNormalizado)
            {
                if (char.IsDigit(caracter))
                {
                    cantidadNumeros++;
                    continue;
                }

                if (
                    caracter != '+' &&
                    caracter != '-' &&
                    caracter != '(' &&
                    caracter != ')' &&
                    !char.IsWhiteSpace(caracter)
                )
                {
                    throw new SumariosDomainException(
                        "El teléfono del representante no es válido."
                    );
                }
            }

            if (cantidadNumeros < 8 || cantidadNumeros > 15)
            {
                throw new SumariosDomainException(
                    "El teléfono del representante debe contener " +
                    "entre 8 y 15 números."
                );
            }
        }
    }
}