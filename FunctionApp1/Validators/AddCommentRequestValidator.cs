using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentValidation;
using FunctionApp1.Model;

namespace FunctionApp1.Validators
{
    class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
    {
        public AddCommentRequestValidator()
        {
            RuleFor(x => x.Text).MaximumLength(200);
            RuleFor(x => x.Text).NotNull();
            RuleFor(x => x.Text).NotEmpty();
            RuleFor(x => x.Author).MaximumLength(50);
            RuleFor(x => x.Author).NotNull();
            RuleFor(x => x.Author).NotEmpty();
        }
    }
}
