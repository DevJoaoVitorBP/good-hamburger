using GoodHamburger.Web.Integration.Models;

namespace GoodHamburger.Web.Components.Pages;

public class MenuState
{
    public bool IsBusy { get; set; }
    public string? ErrorMessage { get; private set; }

    public IReadOnlyCollection<MenuItemResponse> Items { get; set; }
        = Array.Empty<MenuItemResponse>();

    public void SetError(string message)
    {
        ErrorMessage = message;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }
}
