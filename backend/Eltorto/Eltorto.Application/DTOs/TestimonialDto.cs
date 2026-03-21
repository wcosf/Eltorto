namespace Eltorto.Application.DTOs;

public class TestimonialDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Response { get; set; }
    public bool IsApproved { get; set; }
}

public class TestimonialListDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
}

public class CreateTestimonialDto
{
    public string Author { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class UpdateTestimonialDto
{
    public int Id { get; set; }
    public string? Response { get; set; }
    public bool IsApproved { get; set; }
}