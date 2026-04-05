namespace Eltorto.Application.DTOs;

public class SliderItemDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public int SortOrder { get; set; }
}

public class CreateSliderItemDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateSliderItemDto : CreateSliderItemDto
{
    public int Id { get; set; }
}