using Eltorto.Domain.Entities;

public class ContentBlock
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public Page Page { get; set; }
}
