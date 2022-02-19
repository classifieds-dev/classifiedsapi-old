using Shared.Models;
using FluentValidation;

namespace AdsApi.Validators
{
    public class AdValidator : AbstractValidator<Ad>
    {
        public AdValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;
            RuleFor(a => a.AdType).IsInEnum();
            RuleFor(a => a.Status).IsInEnum();
            RuleFor(a => a.Title).NotNull().Length(1, 56);
            RuleFor(a => a.Description).NotNull();
            RuleFor(a => a.Location)
                .NotNull()
                .Must(l => l.Count == 2).WithMessage("Location must contain 2 values.");
            RuleForEach(a => a.Images)
                .Must(i => i.Id != null).WithMessage("Image {CollectionIndex} must have an id.")
                // .Must(i => i.Id.Length == 24).WithMessage("Image {CollectionIndex} id must be 24 characters.")
                .Must(i => i.Path != null).WithMessage("Image {CollectionIndex} must have a path.");
        }
    }
}