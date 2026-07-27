using System;
using System.Collections.Generic;
using System.Linq;

namespace IERIC.SumariosIERIC.Domain.ValueObjects.Network
{
    public class WebAddress : ValueObject
    {
        public string Address { get; private set; }
        public string Scheme { get; private set; }
        public string Host { get; private set; }

        public WebAddress() { }
        public WebAddress(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("La URL no puede ser nula o vacía");
            if (!IsValidUrl(ref url)) throw new ArgumentException("La URL es inválida");

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            Address = url;
            if (uri.IsAbsoluteUri)
            {
                Scheme = uri.Scheme;
                Host = uri.Host;
            }
        }

        public static WebAddress FromUrl(string url)
        {
            return new WebAddress(url);
        }

        /// <summary>
        /// Checks if a given Url is valid.
        /// Valid URLs should include all of the following “prefixes”: https, http, www
        /// - Url must contain http:// or https://
        /// - Url may contain only one instance of www.
        /// - Url Host name type must be Dns
        /// - Url max length is 100
        /// </summary>
        /// <param name="url">Web site Url</param>
        /// <returns><c>true</c> if the Url is valid, otherwise <c>false</c></returns>
        private static bool IsValidUrl(ref string url)
        {
            if (url.StartsWith("www."))
            {
                url = "http://" + url;
            }
            return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
            return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp
                      || uriResult.Scheme == Uri.UriSchemeHttps)
                     && uriResult.Host.Replace("www.", "").Split('.').Count() > 1
                     && uriResult.HostNameType == UriHostNameType.Dns
                     && uriResult.Host.Length > uriResult.Host.LastIndexOf(".") + 1
                     && 100 >= url.Length;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Address.ToLower();
        }

        public static implicit operator string(WebAddress webAddress)
        {
            return webAddress.Address;
        }

        public static explicit operator WebAddress(string url)
        {
            return new WebAddress(url);
        }
    }
}