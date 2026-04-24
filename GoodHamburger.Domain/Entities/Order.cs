namespace GoodHamburger.Domain.Entities;

public sealed class Order
{
    public int Id { get; init; }
    public IReadOnlyList<MenuItem> Items { get; private set; } = [];
    public decimal Subtotal { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Order Create(int id, IReadOnlyList<MenuItem> items, decimal subtotal, decimal discountPercentage, decimal discountAmount, decimal total)
    {
        return new Order
        {
            Id = id,
            Items = items,
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            Total = total,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(IReadOnlyList<MenuItem> items, decimal subtotal, decimal discountPercentage, decimal discountAmount, decimal total)
    {
        Items = items;
        Subtotal = subtotal;
        DiscountPercentage = discountPercentage;
        DiscountAmount = discountAmount;
        Total = total;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
