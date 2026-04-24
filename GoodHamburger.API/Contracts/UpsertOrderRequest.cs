namespace GoodHamburger.API.Contracts;

public sealed class UpsertOrderRequest
{
    public IReadOnlyCollection<int> ItemIds { get; init; } = [];
}
