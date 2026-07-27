using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Location
{
    public class Coordinates : ValueObject
    {
        const float EarthRadiusInKilometers = 6378.0F;
        private Latitude latitude;
        private Longitude longitude;

        public float LatitudeValue
        {
            get
            {
                return latitude.Value;
            }
            set
            {
                latitude = Latitude.FromScalar(value);
            }
        }

        public float LongitudValue
        {
            get
            {
                return longitude.Value;
            }
            set
            {
                longitude = Longitude.FromScalar(value);
            }
        }


        public Coordinates() { }
        public Coordinates(Latitude latitude, Longitude longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public float StraightDistanceInKilometersTo(Coordinates position)
        {
            /// Haversine formula
            var latitudeInRadian = ToRadian(position.latitude - latitude);
            var longitudeInRadian = ToRadian(position.longitude - longitude);
            var a = Math.Pow(Math.Sin(latitudeInRadian / 2), 2) +
                    Math.Cos(ToRadian(latitudeInRadian)) *
                    Math.Cos(ToRadian(position.latitude)) *
                    Math.Pow(Math.Sin(longitudeInRadian / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusInKilometers * Convert.ToSingle(c);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return latitude;
            yield return longitude;
        }

        private float ToRadian(float value)
        {
            return Convert.ToSingle(Math.PI / 180) * value;
        }


    }
}