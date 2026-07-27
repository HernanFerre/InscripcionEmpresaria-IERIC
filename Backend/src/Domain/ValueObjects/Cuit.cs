using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IERIC.SumariosIERIC.Domain.Exceptions;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Network
{
    public class Cuit : ValueObject
    {
        private Int64 NumeroDeCUIT { get; set; }

        public Cuit() { }
        public Cuit(Int64 numeroDeCuit)
        {
            if (!EsCuitValido(numeroDeCuit)) throw new SumariosDomainException("El número de CUIT ingresado no es valido");
            NumeroDeCUIT = numeroDeCuit;
        }

        public static bool EsCuitValido(Int64 cuit)
        {

            // Convertir el número a string
            string cuitStr = cuit.ToString();

            // Validar que el CUIT tenga 11 dígitos
            if (cuitStr.Length != 11)
            {
                return false;
            }

            // Validar el dígito verificador
            int[] multiplicadores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < multiplicadores.Length; i++)
            {
                suma += int.Parse(cuitStr[i].ToString()) * multiplicadores[i];
            }

            int resto = suma % 11;
            int digitoVerificador = resto == 0 ? 0 : resto == 1 ? 9 : 11 - resto;

            return digitoVerificador == int.Parse(cuitStr[10].ToString());
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return NumeroDeCUIT;
        }

        public Int64 ToInt64()
        {
            return this.NumeroDeCUIT;
        }

        public static implicit operator Int64(Cuit c) => c.NumeroDeCUIT;
        public static explicit operator Cuit(Int64 c) => new Cuit(c);

        public override string ToString() => $"{NumeroDeCUIT}";
    }
}