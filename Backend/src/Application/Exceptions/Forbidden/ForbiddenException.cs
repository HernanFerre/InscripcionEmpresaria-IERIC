using System;

namespace IERIC.SumariosIERIC.Application.Exceptions
{
    public class ForbiddenException : Exception, IForbiddenException
    {

        public string Url { get => "https://miDominio/api/v1/login"; }


        public ForbiddenException()
        { }

        public ForbiddenException(string message)
            : base(message)
        { }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        { }

    }
}
