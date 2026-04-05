namespace Eltorto.Application.DTOs;

public class PageDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string? Subheading { get; set; }
    public string? Content { get; set; }
    public List<ContentBlockDto> ContentBlocks { get; set; } = new();
}

public class UpdatePageDto
{
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string? Subheading { get; set; }
    public string? Content { get; set; }
}