namespace Eltorto.Domain.Entities;

public class Filling
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool HasCrossSection { get; set; }
    public ICollection<Cake> Cakes { get; set; } = new List<Cake>();
}