using System;
using Shared.Models;
using FluentValidation;

namespace TaxonomyApi.Validators
{
    public class VocabularyValidator : AbstractValidator<Vocabulary>
    {
        public VocabularyValidator()
        {
        }
    }
}
