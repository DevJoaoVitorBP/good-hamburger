using GoodHamburger.Web.Integration;

namespace GoodHamburger.Web.Components.Pages;

public class OrdersState
{
    public bool IsBusy { get; set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public IReadOnlyCollection<GoodHamburgerApiClient.MenuItemResponse> MenuItems { get; set; }
        = Array.Empty<GoodHamburgerApiClient.MenuItemResponse>();

    public IReadOnlyCollection<GoodHamburgerApiClient.OrderResponse> Orders { get; set; }
        = Array.Empty<GoodHamburgerApiClient.OrderResponse>();

    public HashSet<int> CreateItemIds { get; set; } = new();
    public HashSet<int> UpdateItemIds { get; set; } = new();

    public int UpdateOrderId { get; set; }

    public void SetError(string message)
    {
        ErrorMessage = message;
        SuccessMessage = null;
    }

    public void SetSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage = null;
    }

    public void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
