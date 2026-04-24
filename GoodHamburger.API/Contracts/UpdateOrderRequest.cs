namespace GoodHamburger.API.Contracts;

public sealed class UpdateOrderRequest
{
    public IReadOnlyCollection<int> ItemIds { get; init; } = [];
}
