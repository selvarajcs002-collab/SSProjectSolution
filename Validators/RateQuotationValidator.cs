using FluentValidation;
using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Validators
{
    public class RateQuotationCreateDtoValidator : AbstractValidator<RateQuotationCreateDto>
    {
        public RateQuotationCreateDtoValidator()
        {
            RuleFor(x => x.RatePerPiece)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.RatePerPiece));

            RuleFor(x => x.RatePerMeter)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.RatePerMeter));

            RuleFor(x => x.TotalAmount)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.TotalAmount));
        }
    }

    public class RateQuotationUpdateDtoValidator : AbstractValidator<RateQuotationUpdateDto>
    {
        public RateQuotationUpdateDtoValidator()
        {
            RuleFor(x => x.RatePerPiece)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.RatePerPiece));

            RuleFor(x => x.RatePerMeter)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.RatePerMeter));

            RuleFor(x => x.TotalAmount)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.TotalAmount));
        }
    }
}
