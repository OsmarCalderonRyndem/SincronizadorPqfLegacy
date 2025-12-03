namespace SincronizadorPqfLegacy.Domain.Exceptions
{
    public class InfrastructureNotImplementedException : NotImplementedException
    {
        public InfrastructureNotImplementedException() : base("This method is not implemented in the Infrastructure layer.") { }
    }
}
