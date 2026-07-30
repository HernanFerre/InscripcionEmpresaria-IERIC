using System.Collections.Generic;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Domain.SeedWork;
using IERIC.SumariosIERIC.Domain.ValueObjects.Network;

namespace IERIC.SumariosIERIC.Domain.ValueObjects
{
    public sealed class EstadoInscripcionEmpresa
        : ValueObject
    {
        private static readonly HashSet<int>
            CodigosHabilitados =
                new HashSet<int>
                {
                    5,
                    8,
                    9
                };

        public Cuit CuitEmpresa { get; private set; }

        public string RazonSocial { get; private set; }

        public int CodigoEstado { get; private set; }

        public string Mensaje { get; private set; }

        public bool PuedeIniciarInscripcion =>
            CodigosHabilitados.Contains(
                CodigoEstado
            );

        private EstadoInscripcionEmpresa()
        {
        }

        public EstadoInscripcionEmpresa(
            Cuit cuitEmpresa,
            string razonSocial,
            int codigoEstado,
            string mensaje
        )
        {
            if (cuitEmpresa == null)
            {
                throw new SumariosDomainException(
                    "El estado debe estar asociado a un CUIT."
                );
            }

            CuitEmpresa = cuitEmpresa;

            RazonSocial =
                string.IsNullOrWhiteSpace(razonSocial)
                    ? null
                    : razonSocial.Trim();

            CodigoEstado = codigoEstado;

            Mensaje =
                mensaje?.Trim() ??
                string.Empty;
        }

        protected override IEnumerable<object>
            GetAtomicValues()
        {
            yield return CuitEmpresa;
            yield return RazonSocial;
            yield return CodigoEstado;
            yield return Mensaje;
        }
    }
}