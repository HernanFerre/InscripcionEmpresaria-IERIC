using System;

namespace IERIC.SumariosIERIC.Application.Exceptions
{
    public class NotFoundException : Exception, INotFoundException
    {
        public string ID { get; set; }
        public NotFoundException()
        { }

        public NotFoundException(string message, string id)
            : base(message)
        {
            ID = id;
        }

        public NotFoundException(string message, string id, Exception innerException)
            : base(message, innerException)
        {
            ID = id;
        }

    }
}
