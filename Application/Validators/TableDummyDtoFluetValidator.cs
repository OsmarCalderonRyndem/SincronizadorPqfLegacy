using SincronizadorPqfLegacy.Application.DTOs;
using FluentValidation;

namespace SincronizadorPqfLegacy.Application.Validators
{
    public class TableDummyDtoFluetValidator : AbstractValidator<TableDummyDto>
    {
        public TableDummyDtoFluetValidator()
        {
            RuleFor(x => x.IdRegister)
                .NotEmpty().WithMessage("IdRegister is required.")
                .NotEqual(Guid.Empty).WithMessage("IdRegister cannot be an empty GUID.");
        }
    }
}
