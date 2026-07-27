using System;
using System.Collections.Generic;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Location
{
    public class Direccion : ValueObject
    {
        private readonly string numero;
        private readonly string calle;
        private readonly string barrio;
        private readonly string localidad;
        private readonly string provincia;
        private readonly string codigoPostal;
        private readonly string pais;

        public string Numero => numero;
        public string Calle => calle;
        public string Barrio => barrio;
        public string Localidad => localidad;
        public string Provincia => provincia;
        public string CodigoPostal => codigoPostal;
        public string Pais => pais;

        public Direccion() { }
        public Direccion(string numero
        , string calle
        , string barrio
        , string localidad
        , string provincia
        , string codigoPostal
        , string pais)
        {
            this.numero = numero;
            this.calle = calle;
            this.barrio = barrio;
            this.localidad = localidad;
            this.provincia = provincia;
            this.codigoPostal = codigoPostal;
            this.pais = pais;

        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return numero;
            yield return calle;
            yield return barrio;
            yield return localidad;
            yield return provincia;
            yield return codigoPostal;
            yield return pais;

        }

        public override string ToString()
        {
            return calle + " " + numero + ", " + codigoPostal + " " + localidad + ", " + pais;
        }

        public List<AddressComponent> toAddressComponents()
        {
            List<AddressComponent> retorno = new List<AddressComponent>();
            if (numero != string.Empty) retorno.Add(new AddressComponent() { short_name = numero, long_name = numero, types = new List<string> { "street_number" } });
            if (calle != string.Empty) retorno.Add(new AddressComponent() { short_name = calle, long_name = calle, types = new List<string> { "route" } });
            if (barrio != string.Empty) retorno.Add(new AddressComponent() { short_name = barrio, long_name = barrio, types = new List<string> { "neighborhood" } });
            if (provincia != string.Empty) retorno.Add(new AddressComponent()
            {
                short_name = provincia,
                long_name = provincia,
                types = new List<string> { "administrative_area_level_1" }
            });
            if (localidad != string.Empty) retorno.Add(new AddressComponent()
            {
                short_name = localidad,
                long_name = localidad,
                types = new List<string> { "administrative_area_level_2" }
            });
            if (pais != string.Empty) retorno.Add(new AddressComponent()
            {
                short_name = pais,
                long_name = pais,
                types = new List<string> { "country" }
            });
            if (codigoPostal != string.Empty) retorno.Add(new AddressComponent()
            {
                short_name = codigoPostal,
                long_name = codigoPostal,
                types = new List<string> { "postal_code" }
            });


            return retorno;

        }

        public static Direccion fromAddressComponents(List<AddressComponent> addressComponents)
        {
            Direccion retorno = new Direccion(
            numero: addressComponents.Find(i => i.types.Contains("street_number"))?.long_name,
            calle: addressComponents.Find(i => i.types.Contains("route"))?.long_name,
            barrio: addressComponents.Find(i => i.types.Contains("sublocality") || i.types.Contains("neighborhood") || i.types.Contains("locality"))?.long_name,
            provincia: addressComponents.Find(i => i.types.Contains("administrative_area_level_1"))?.long_name,
            localidad: addressComponents.Find(i => i.types.Contains("administrative_area_level_2"))?.long_name,
            pais: addressComponents.Find(i => i.types.Contains("country"))?.long_name,
            codigoPostal: addressComponents.Find(i => i.types.Contains("postal_code"))?.long_name

            );
            return retorno;
        }
        public class AddressComponent
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public List<string> types { get; set; } = new List<string>();
        }

    }
}