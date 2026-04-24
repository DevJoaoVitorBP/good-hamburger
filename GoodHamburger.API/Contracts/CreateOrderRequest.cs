namespace GoodHamburger.API.Contracts;

public sealed class CreateOrderRequest
{
    public IReadOnlyCollection<int> ItemIds { get; init; } = [];
}
