namespace Eltorto.Domain.Entities;

public class SliderItem
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public int SortOrder { get; set; }
}