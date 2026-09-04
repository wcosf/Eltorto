using FluentValidation;
using Eltorto.Application.DTOs;

namespace Eltorto.Application.Validators;

public class CreateTestimonialDtoValidator : AbstractValidator<CreateTestimonialDto>
{
    public CreateTestimonialDtoValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Имя автора обязательно")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(100).WithMessage("Email не должен превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст отзыва обязателен")
            .MinimumLength(3).WithMessage("Отзыв должен содержать не менее 3 символов")
            .MaximumLength(2000).WithMessage("Отзыв не должен превышать 2000 символов");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Оценка должна быть от 1 до 5")
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.RecaptchaToken)
            .NotEmpty().WithMessage("Капча обязательна");
    }
}

public class UpdateTestimonialDtoValidator : AbstractValidator<UpdateTestimonialDto>
{
    public UpdateTestimonialDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Некорректный ID отзыва");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Имя автора обязательно")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст отзыва обязателен")
            .MinimumLength(3).WithMessage("Отзыв должен содержать не менее 3 символов")
            .MaximumLength(2000).WithMessage("Отзыв не должен превышать 2000 символов");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Оценка должна быть от 1 до 5")
            .When(x => x.Rating.HasValue);
    }
}


