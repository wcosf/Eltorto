namespace Eltorto.Application.DTOs;

public class CakeListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public string? Description { get; set; }
    public int? FillingId { get; set; }
    public string? FillingName { get; set; }
}

public class CakeDetailDto : CakeListDto
{
    public string? SubCategory { get; set; }
    public FillingDto? Filling { get; set; }
}

public class CreateCakeDto
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public bool IsFeatured { get; set; }
    public string? Description { get; set; }
    public int? FillingId { get; set; }
}

public class UpdateCakeDto : CreateCakeDto
{
    public int Id { get; set; }
}