namespace Eltorto.Domain.Entities;

public class Testimonial
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Response { get; set; }
    public bool IsApproved { get; set; }
}