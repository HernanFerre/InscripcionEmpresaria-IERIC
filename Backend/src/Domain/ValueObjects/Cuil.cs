using System;
using System.Collections.Generic;
using IERIC.SumariosIERIC.Domain.Exceptions;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Network
{
    public class Cuil : ValueObject
    {
        private Int64 NumeroDeCuil { get; set; }

        public Cuil()
        {
        }

        public Cuil(Int64 numeroDeCuil)
        {
            if (!EsCuilValido(numeroDeCuil))
            {
                throw new SumariosDomainException(
                    "El número de CUIL ingresado no es válido"
                );
            }

            NumeroDeCuil = numeroDeCuil;
        }

        public static bool EsCuilValido(Int64 cuil)
        {
            string cuilString = cuil.ToString();

            if (cuilString.Length != 11)
            {
                return false;
            }

            int[] multiplicadores =
            {
                5, 4, 3, 2, 7, 6, 5, 4, 3, 2
            };

            int suma = 0;

            for (int i = 0; i < multiplicadores.Length; i++)
            {
                suma +=
                    int.Parse(cuilString[i].ToString()) *
                    multiplicadores[i];
            }

            int resto = suma % 11;

            int digitoVerificador =
                resto == 0
                    ? 0
                    : resto == 1
                        ? 9
                        : 11 - resto;

            return digitoVerificador ==
                   int.Parse(cuilString[10].ToString());
        }

        public Int64 ToInt64()
        {
            return NumeroDeCuil;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return NumeroDeCuil;
        }

        public static implicit operator Int64(Cuil cuil)
        {
            return cuil.NumeroDeCuil;
        }

        public static explicit operator Cuil(Int64 numero)
        {
            return new Cuil(numero);
        }

        public override string ToString()
        {
            return NumeroDeCuil.ToString();
        }
    }
}