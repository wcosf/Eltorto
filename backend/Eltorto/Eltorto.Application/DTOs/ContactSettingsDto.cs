namespace Eltorto.Application.DTOs;

public class ContactSettingsDto
{
    public int Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? AdditionalPhone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? MapUrl { get; set; }
}

public class UpdateContactSettingsDto
{
    public string Phone { get; set; } = string.Empty;
    public string? AdditionalPhone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? MapUrl { get; set; }
}