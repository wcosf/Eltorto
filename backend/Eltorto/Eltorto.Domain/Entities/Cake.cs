namespace Eltorto.Domain.Entities;

public class Cake
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string? SubCategory { get; set; }
    public bool IsFeatured { get; set; }
    public string? Description { get; set; }
    public int? FillingId { get; set; }
    public Filling? Filling { get; set; }
}