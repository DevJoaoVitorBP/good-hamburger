using GoodHamburger.Web.Integration;

namespace GoodHamburger.Web.Components.Pages;

public class MenuState
{
    public bool IsBusy { get; set; }
    public string? ErrorMessage { get; private set; }

    public IReadOnlyCollection<GoodHamburgerApiClient.MenuItemResponse> Items { get; set; }
        = Array.Empty<GoodHamburgerApiClient.MenuItemResponse>();

    public void SetError(string message)
    {
        ErrorMessage = message;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }
}
