namespace SincronizadorPqfLegacy.Application.Exceptions
{
    public class AppKeyNotFoundException : KeyNotFoundException
    {
        public AppKeyNotFoundException() : base() { }
        public AppKeyNotFoundException(string message) : base(message) { }
        public AppKeyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
