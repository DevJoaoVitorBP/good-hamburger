namespace GoodHamburger.Application.Contracts;

public sealed record OrderItemDto(int Id, string Name, string Category, decimal Price);
