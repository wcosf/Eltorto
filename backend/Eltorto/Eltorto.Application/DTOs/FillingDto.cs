namespace Eltorto.Application.DTOs;

public class FillingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool HasCrossSection { get; set; }
}

public class FillingWithCakesDto : FillingDto
{
    public List<CakeListDto> Cakes { get; set; } = new();
}

public class CreateFillingDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool HasCrossSection { get; set; }
}

public class UpdateFillingDto : CreateFillingDto
{
    public int Id { get; set; }
}