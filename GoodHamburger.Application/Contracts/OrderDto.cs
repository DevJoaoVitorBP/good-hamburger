namespace GoodHamburger.Application.Contracts;

public sealed record OrderDto(
    int Id,
    IReadOnlyCollection<OrderItemDto> Items,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
