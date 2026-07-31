using FluentValidation;
using Eltorto.Application.DTOs;

namespace Eltorto.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Имя клиента обязательно")
            .MaximumLength(200).WithMessage("Имя не должно превышать 200 символов");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .MaximumLength(20).WithMessage("Номер не должен превышать 20 символов")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Некорректный формат номера телефона");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(100).WithMessage("Email не должен превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));

        RuleFor(x => x.CakeId)
            .GreaterThan(0).WithMessage("Некорректный ID торта")
            .When(x => x.CakeId.HasValue);

        RuleFor(x => x.CustomCakeDescription)
            .MaximumLength(2000).WithMessage("Описание не должно превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.CustomCakeDescription));

        RuleFor(x => x.FillingId)
            .GreaterThan(0).WithMessage("Некорректный ID начинки")
            .When(x => x.FillingId.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Вес должен быть положительным")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.DeliveryAddress)
            .MaximumLength(500).WithMessage("Адрес не должен превышать 500 символов")
            .When(x => !string.IsNullOrEmpty(x.DeliveryAddress));

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Комментарий не должен превышать 1000 символов")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Статус обязателен")
            .MaximumLength(50).WithMessage("Статус не должен превышать 50 символов");
    }
}
