namespace Microservicio.Application.Exceptions
{
    public class AppArgumentNullException : ArgumentNullException
    {
        public AppArgumentNullException(string paramName) : base(paramName) { }
        public AppArgumentNullException(string paramName, string message) : base(paramName, message) { }
        public AppArgumentNullException(string message, Exception innerException) : base(message, innerException) { }
    }
}
