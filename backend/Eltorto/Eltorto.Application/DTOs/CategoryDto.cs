namespace Eltorto.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class CategoryWithCakesDto : CategoryDto
{
    public List<CakeListDto> Cakes { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto
{
    public int Id { get; set; }
}