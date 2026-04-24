using System.Globalization;
using GoodHamburger.Web.Integration;
using Microsoft.AspNetCore.Components;

namespace GoodHamburger.Web.Components.Pages;

public partial class Menu : ComponentBase
{
    [Inject] private GoodHamburgerApiClient ApiClient { get; set; } = default!;

    private bool _isBusy;
    private string? _errorMessage;
    private IReadOnlyCollection<GoodHamburgerApiClient.MenuItemResponse> _items = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadMenuAsync();
    }

    private async Task LoadMenuAsync()
    {
        _isBusy = true;
        _errorMessage = null;

        try
        {
            _items = await ApiClient.GetMenuAsync();
        }
        catch
        {
            _errorMessage = "Unable to load the menu at the moment. Please try again.";
        }
        finally
        {
            _isBusy = false;
        }
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C", new CultureInfo("pt-BR"));
    }
}
