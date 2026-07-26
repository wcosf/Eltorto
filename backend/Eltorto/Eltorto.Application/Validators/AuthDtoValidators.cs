using FluentValidation;
using Eltorto.Application.DTOs;

namespace Eltorto.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно")
            .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Имя пользователя может содержать только латинские буквы, цифры, _ и -");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(100).WithMessage("Email не должен превышать 100 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву")
            .Matches("[a-z]").WithMessage("Пароль должен содержать строчную букву")
            .Matches(@"\d").WithMessage("Пароль должен содержать цифру")
            .Matches(@"[^\da-zA-Z]").WithMessage("Пароль должен содержать специальный символ");

        RuleFor(x => x.FullName)
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.FullName));
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Текущий пароль обязателен");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Новый пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен быть не менее 8 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву")
            .Matches("[a-z]").WithMessage("Пароль должен содержать строчную букву")
            .Matches(@"\d").WithMessage("Пароль должен содержать цифру")
            .Matches(@"[^\da-zA-Z]").WithMessage("Пароль должен содержать специальный символ");
    }
}

public class ChangeUserNameRequestValidator : AbstractValidator<ChangeUserNameRequest>
{
    public ChangeUserNameRequestValidator()
    {
        RuleFor(x => x.NewUserName)
            .NotEmpty().WithMessage("Новое имя пользователя обязательно")
            .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов")
            .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов")
            .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Имя пользователя может содержать только латинские буквы, цифры, _ и -");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен для подтверждения");
    }
}