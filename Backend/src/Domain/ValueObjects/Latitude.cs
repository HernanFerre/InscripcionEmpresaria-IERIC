using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Location
{
    public class Latitude : ValueObject
    {
        private readonly float value;

        public float Value => value;

        public Latitude() { }
        private Latitude(float value)
        {
            if (value < -90 || value > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "La latitud debe estar en el rango [-90; 90]");
            }

            this.value = value;
        }

        public static Latitude FromScalar(float value)
        {
            return new Latitude(value);
        }

        public static implicit operator float(Latitude latitude)
        {
            return latitude.value;
        }

        public static explicit operator Latitude(float value)
        {
            return new Latitude(value);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}