using System.Globalization;
using GoodHamburger.Web.Integration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoodHamburger.Web.Components.Pages;

public partial class Orders : ComponentBase
{
    [Inject] private GoodHamburgerApiClient ApiClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool _isBusy;
    private string? _errorMessage;
    private string? _successMessage;

    private IReadOnlyCollection<GoodHamburgerApiClient.MenuItemResponse> _menuItems = [];
    private IReadOnlyCollection<GoodHamburgerApiClient.OrderResponse> _orders = [];
    private readonly HashSet<int> _createItemIds = [];
    private readonly HashSet<int> _updateItemIds = [];
    private int _updateOrderId;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await RunBusyOperationAsync(async () =>
        {
            _menuItems = await ApiClient.GetMenuAsync();
            _orders = await ApiClient.GetOrdersAsync();
        });
    }

    private void ToggleCreateSelection(int itemId, ChangeEventArgs args)
    {
        ToggleSelection(_createItemIds, itemId, args);
    }

    private void ToggleUpdateSelection(int itemId, ChangeEventArgs args)
    {
        ToggleSelection(_updateItemIds, itemId, args);
    }

    private static void ToggleSelection(HashSet<int> target, int itemId, ChangeEventArgs args)
    {
        bool selected = args.Value is true || (args.Value is string value && bool.TryParse(value, out bool parsed) && parsed);

        if (selected)
        {
            target.Add(itemId);
            return;
        }

        target.Remove(itemId);
    }

    private async Task CreateOrderAsync()
    {
        await RunBusyOperationAsync(async () =>
        {
            await ApiClient.CreateOrderAsync(_createItemIds.ToList());
            _createItemIds.Clear();
            _successMessage = "Order created successfully.";
            _orders = await ApiClient.GetOrdersAsync();
        });
    }

    private async Task LoadOrderForEditAsync()
    {
        if (_updateOrderId <= 0)
        {
            _errorMessage = "Please enter a valid order ID.";
            return;
        }

        await RunBusyOperationAsync(async () =>
        {
            GoodHamburgerApiClient.OrderResponse? order = await ApiClient.GetOrderByIdAsync(_updateOrderId);
            if (order is null)
            {
                _errorMessage = "We could not find an order with this ID.";
                return;
            }

            _updateItemIds.Clear();
            foreach (GoodHamburgerApiClient.OrderItemResponse item in order.Items)
            {
                _updateItemIds.Add(item.Id);
            }

            _successMessage = $"Order {_updateOrderId} loaded.";
        });
    }

    private async Task UpdateOrderAsync()
    {
        if (_updateOrderId <= 0)
        {
            _errorMessage = "Please enter a valid order ID.";
            return;
        }

        if (_updateItemIds.Count == 0)
        {
            _errorMessage = "Please select at least one item before updating the order.";
            return;
        }

        await RunBusyOperationAsync(async () =>
        {
            await ApiClient.UpdateOrderAsync(_updateOrderId, _updateItemIds.ToList());
            _successMessage = "Order updated successfully.";
            _orders = await ApiClient.GetOrdersAsync();
        });
    }

    private void StartEdit(GoodHamburgerApiClient.OrderResponse order)
    {
        _updateOrderId = order.Id;
        _updateItemIds.Clear();

        foreach (GoodHamburgerApiClient.OrderItemResponse item in order.Items)
        {
            _updateItemIds.Add(item.Id);
        }

        _errorMessage = null;
        _successMessage = $"Order {order.Id} selected for edit.";
    }

    private async Task DeleteOrderAsync(int orderId)
    {
        bool confirmed = await JS.InvokeAsync<bool>("confirm", $"Are you sure you want to delete order #{orderId}?");
        if (!confirmed)
        {
            return;
        }

        await RunBusyOperationAsync(async () =>
        {
            await ApiClient.DeleteOrderAsync(orderId);
            _successMessage = "Order deleted successfully.";
            _orders = await ApiClient.GetOrdersAsync();
        });
    }

    private async Task RunBusyOperationAsync(Func<Task> operation)
    {
        _isBusy = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            _errorMessage = ToFriendlyErrorMessage(exception);
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

    private static string FormatDateTime(DateTimeOffset value)
    {
        return value.LocalDateTime.ToString("dd/MM/yyyy HH:mm");
    }

    private static string FormatNullableDateTime(DateTimeOffset? value)
    {
        return value.HasValue ? FormatDateTime(value.Value) : "-";
    }

    private static string ToFriendlyErrorMessage(Exception exception)
    {
        string message = exception.Message;

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return "We could not find the requested resource. Please check the ID and try again.";
        }

        if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
        {
            return "You selected duplicated items. Please keep only one of each item.";
        }

        if (message.Contains("exactly one sandwich", StringComparison.OrdinalIgnoreCase))
        {
            return "Your order must contain exactly one sandwich.";
        }

        return "Something went wrong while processing your request. Please try again.";
    }
}
