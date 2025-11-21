namespace Microservicio.Domain.Exceptions
{
    public class DomainArgumentNullException : Exception
    {
        public DomainArgumentNullException() { }
        public DomainArgumentNullException(string message) : base(message) { }
        public DomainArgumentNullException(string message, Exception inner) : base(message, inner) { }
    }
}
