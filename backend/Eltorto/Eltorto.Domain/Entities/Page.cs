namespace Eltorto.Domain.Entities;

public class Page
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string? Subheading { get; set; }
    public string? Content { get; set; }
    public ICollection<ContentBlock> ContentBlocks { get; set; } = new List<ContentBlock>();
}