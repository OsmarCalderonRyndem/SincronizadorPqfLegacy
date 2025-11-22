namespace SincronizadorPqfLegacy.Application.Exceptions
{
    public class AppFileNotFoundException : FileNotFoundException
    {
        public AppFileNotFoundException() : base() { }
        public AppFileNotFoundException(string message) : base(message) { }
        public AppFileNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
