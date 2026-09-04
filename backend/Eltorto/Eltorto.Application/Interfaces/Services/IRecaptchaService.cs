namespace Eltorto.Application.Interfaces.Services;

public interface IRecaptchaService
{
    Task<bool> VerifyTokenAsync(string token, CancellationToken cancellationToken = default);
}
