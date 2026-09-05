using FluentValidation;
using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Validators
{
    public class RateQuotationCreateDtoValidator : AbstractValidator<RateQuotationCreateDto>
    {
        public RateQuotationCreateDtoValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("CompanyId is mandatory and must be greater than 0.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("CompanyName is mandatory.");

            RuleFor(x => x.QuotationDate)
                .NotEmpty().WithMessage("QuotationDate is mandatory.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.RatePerPiece)
                .MaximumLength(200).WithMessage("RatePerPiece cannot exceed 200 characters.");

            RuleFor(x => x.RatePerMeter)
                .MaximumLength(200).WithMessage("RatePerMeter cannot exceed 200 characters.");

            RuleFor(x => x.MobileNo)
                .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.MobileNo))
                .WithMessage("Invalid mobile number format.");

            RuleFor(x => x.EmailId)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.EmailId))
                .WithMessage("Invalid email format.");
                
            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("TotalAmount cannot be negative.");
        }
    }

    public class RateQuotationUpdateDtoValidator : AbstractValidator<RateQuotationUpdateDto>
    {
        public RateQuotationUpdateDtoValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("CompanyId is mandatory and must be greater than 0.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("CompanyName is mandatory.");

            RuleFor(x => x.QuotationDate)
                .NotEmpty().WithMessage("QuotationDate is mandatory.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.RatePerPiece)
                .MaximumLength(200).WithMessage("RatePerPiece cannot exceed 200 characters.");

            RuleFor(x => x.RatePerMeter)
                .MaximumLength(200).WithMessage("RatePerMeter cannot exceed 200 characters.");

            RuleFor(x => x.MobileNo)
                .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.MobileNo))
                .WithMessage("Invalid mobile number format.");

            RuleFor(x => x.EmailId)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.EmailId))
                .WithMessage("Invalid email format.");
                
            RuleFor(x => x.TotalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("TotalAmount cannot be negative.");
        }
    }
}
