using System.Globalization;
using GoodHamburger.Web.Integration;
using GoodHamburger.Web.Integration.Models;
using Microsoft.AspNetCore.Components;

namespace GoodHamburger.Web.Components.Pages;

public partial class Menu : ComponentBase
{
    [Inject] private GoodHamburgerApiClient ApiClient { get; set; } = default!;

    private readonly MenuState _state = new();
    private bool _isBusy => _state.IsBusy;
    private string? _errorMessage => _state.ErrorMessage;
    private IReadOnlyCollection<MenuItemResponse> _items => _state.Items;

    protected override async Task OnInitializedAsync()
    {
        await LoadMenuAsync();
    }

    private async Task LoadMenuAsync()
    {
        _state.IsBusy = true;
        _state.ClearError();

        try
        {
            _state.Items = await ApiClient.GetMenuAsync();
        }
        catch
        {
            _state.SetError("Unable to load the menu at the moment. Please try again.");
        }
        finally
        {
            _state.IsBusy = false;
        }
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C", new CultureInfo("pt-BR"));
    }
}
