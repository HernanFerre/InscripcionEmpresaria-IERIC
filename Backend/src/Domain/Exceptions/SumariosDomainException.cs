using System;

namespace IERIC.SumariosIERIC.Domain.Exceptions
{
    /// <summary>
    /// Exception type for domain exceptions
    /// </summary>
    public class SumariosDomainException : Exception
    {
        public SumariosDomainException()
        { }

        public SumariosDomainException(string message)
            : base(message)
        { }

        public SumariosDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}