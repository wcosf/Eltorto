namespace Eltorto.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SortOrder { get; set; } = "0";

    public ICollection<Cake> Cakes { get; set; } = new List<Cake>();
}