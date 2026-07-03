using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Slug обязателен")]
    [RegularExpression(@"^[a-z0-9-]+$",
        ErrorMessage = "Slug может содержать только латинские буквы (a-z), цифры (0-9) и дефис (-).")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Название обязательно")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Порядок сортировки должен быть неотрицательным числом")]
    public int SortOrder { get; set; }
}

public class UpdateCategoryDto : CreateCategoryDto
{
    public int Id { get; set; }
}