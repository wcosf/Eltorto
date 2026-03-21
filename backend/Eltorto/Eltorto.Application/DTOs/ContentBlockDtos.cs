namespace Eltorto.Application.DTOs;

public class ContentBlockDto
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}

public class CreateContentBlockDto
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateContentBlockDto : CreateContentBlockDto
{
    public int Id { get; set; }
}