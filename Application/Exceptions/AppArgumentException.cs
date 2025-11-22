namespace SincronizadorPqfLegacy.Application.Exceptions
{
    public class AppArgumentException : ArgumentException
    {
        public AppArgumentException() : base() { }
        public AppArgumentException(string message, string v) : base(message) { }
        public AppArgumentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
