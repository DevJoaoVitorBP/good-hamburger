namespace GoodHamburger.Web.Integration.Models;

public sealed record OrderResponse(
        int Id,
        IReadOnlyCollection<OrderItemResponse> Items,
        decimal Subtotal,
        decimal DiscountPercentage,
        decimal DiscountAmount,
        decimal Total,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);