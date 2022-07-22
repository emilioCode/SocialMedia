using FluentValidation;
using SocialMedia.Core.DTOs;
using System;

namespace SocialMedia.Infrastructure.Validators
{
    public class PostValidator: AbstractValidator<PostDto>
    {
        public PostValidator()
        {
            RuleFor(post => post.Description)
                .NotNull().WithMessage("Description is empty")
                .Length(10, 1000).WithMessage("the length should be between 10 and 1000 characters");

            RuleFor(post => post.Date)
                .NotNull()
               .LessThan(DateTime.Now);
        }
    }
}
