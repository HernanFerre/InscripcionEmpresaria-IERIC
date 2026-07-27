using System;

namespace IERIC.SumariosIERIC.Application.Exceptions
{
    public class InvalidException : Exception, IInvalidException
    {
        public string Solution { get; set; }
        public InvalidException()
        { }

        public InvalidException(string message, string solution)
            : base(message)
        {
            Solution = solution;
        }

        public InvalidException(string message, string solution, Exception innerException)
            : base(message, innerException)
        {
            Solution = solution;
        }

    }
}
