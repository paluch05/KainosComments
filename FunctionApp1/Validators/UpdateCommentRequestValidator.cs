using FluentValidation;
using FunctionApp1.Model;

namespace FunctionApp1.Validators
{
    class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
    {
        public UpdateCommentRequestValidator()
        {
            RuleFor(x => x.Text).Length(200);
            RuleFor(x => x.Text).NotEmpty();
            RuleFor(x => x.Text).NotNull();
        }
    }
}
