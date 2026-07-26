using FluentValidation;
using Eltorto.Application.DTOs;

namespace Eltorto.Application.Validators;

public class CreateCakeDtoValidator : AbstractValidator<CreateCakeDto>
{
    public CreateCakeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название торта обязательно")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("URL изображения обязателен")
            .MaximumLength(500).WithMessage("URL не должен превышать 500 символов")
            .Must(BeValidUrl).WithMessage("Некорректный URL изображения")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500).WithMessage("URL не должен превышать 500 символов")
            .Must(BeValidUrl).WithMessage("Некорректный URL превью")
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl));

        RuleFor(x => x.CategorySlug)
            .NotEmpty().WithMessage("Slug категории обязателен")
            .MaximumLength(50).WithMessage("Slug не должен превышать 50 символов")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug может содержать только строчные буквы, цифры и дефис");

        RuleFor(x => x.SubCategory)
            .MaximumLength(50).WithMessage("Подкатегория не должна превышать 50 символов")
            .When(x => !string.IsNullOrEmpty(x.SubCategory));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не должно превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.FillingId)
            .GreaterThan(0).WithMessage("Некорректный ID начинки")
            .When(x => x.FillingId.HasValue);
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateCakeDtoValidator : AbstractValidator<UpdateCakeDto>
{
    public UpdateCakeDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Некорректный ID торта");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название торта обязательно")
            .MaximumLength(100).WithMessage("Название не должно превышать 100 символов");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("URL изображения обязателен")
            .MaximumLength(500).WithMessage("URL не должен превышать 500 символов")
            .Must(BeValidUrl).WithMessage("Некорректный URL изображения")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ThumbnailUrl)
            .MaximumLength(500).WithMessage("URL не должен превышать 500 символов")
            .Must(BeValidUrl).WithMessage("Некорректный URL превью")
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl));

        RuleFor(x => x.CategorySlug)
            .NotEmpty().WithMessage("Slug категории обязателен")
            .MaximumLength(50).WithMessage("Slug не должен превышать 50 символов")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Slug может содержать только строчные буквы, цифры и дефис");

        RuleFor(x => x.SubCategory)
            .MaximumLength(50).WithMessage("Подкатегория не должна превышать 50 символов")
            .When(x => !string.IsNullOrEmpty(x.SubCategory));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не должно превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.FillingId)
            .GreaterThan(0).WithMessage("Некорректный ID начинки")
            .When(x => x.FillingId.HasValue);
    }

    private static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}