using System;
using Shared.Models;
using FluentValidation;

namespace TaxonomyApi.Validators
{
    public class TermValidator : AbstractValidator<Term>
    {
        public TermValidator()
        {
        }
    }
}
