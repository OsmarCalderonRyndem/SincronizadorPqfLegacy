namespace SincronizadorPqfLegacy.Infrastructure.Exceptions
{
    public class InfrastructureNotImplementedException : NotImplementedException
    {
        public InfrastructureNotImplementedException() : base("This method is not implemented in the Infrastructure layer.") { }
    }
}
