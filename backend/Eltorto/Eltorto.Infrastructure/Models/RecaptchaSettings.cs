namespace Eltorto.Infrastructure.Models;

public class RecaptchaSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public double ScoreThreshold { get; set; } = 0.1;
}
