namespace Eltorto.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int? CakeId { get; set; }
    public string? CakeName { get; set; }
    public string? CustomCakeDescription { get; set; }
    public int? FillingId { get; set; }
    public string? FillingName { get; set; }
    public decimal? Weight { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int? CakeId { get; set; }
    public string? CustomCakeDescription { get; set; }
    public int? FillingId { get; set; }
    public decimal? Weight { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? Comment { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}