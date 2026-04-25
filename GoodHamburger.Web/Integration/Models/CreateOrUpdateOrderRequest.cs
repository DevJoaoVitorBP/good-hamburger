namespace GoodHamburger.Web.Integration.Models;

internal sealed record CreateOrUpdateOrderRequest(IReadOnlyCollection<int> ItemIds);