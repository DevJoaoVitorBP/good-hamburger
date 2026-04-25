using System.Globalization;
using GoodHamburger.Web.Integration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GoodHamburger.Web.Components.Pages;

public partial class Orders : ComponentBase
{
    [Inject] private GoodHamburgerApiClient ApiClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private readonly OrdersState _state = new();
    private bool _isBusy => _state.IsBusy;
    private string? _errorMessage => _state.ErrorMessage;
    private string? _successMessage => _state.SuccessMessage;
    private IReadOnlyCollection<GoodHamburgerApiClient.MenuItemResponse> _menuItems => _state.MenuItems;
    private IReadOnlyCollection<GoodHamburgerApiClient.OrderResponse> _orders => _state.Orders;
    private HashSet<int> _createItemIds => _state.CreateItemIds;
    private HashSet<int> _updateItemIds => _state.UpdateItemIds;
    private int _updateOrderId
    {
        get => _state.UpdateOrderId;
        set => _state.UpdateOrderId = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await ExecuteAsync(LoadDataAsync);
    }

    private async Task LoadDataAsync()
    {
        _state.MenuItems = await ApiClient.GetMenuAsync();
        _state.Orders = await ApiClient.GetOrdersAsync();
    }

    private void ToggleCreateSelection(int itemId, bool selected)
    {
        ToggleSelection(_state.CreateItemIds, itemId, selected);
    }

    private void ToggleCreateSelection(int itemId, ChangeEventArgs args)
    {
        ToggleCreateSelection(itemId, ParseCheckboxValue(args));
    }

    private void ToggleUpdateSelection(int itemId, bool selected)
    {
        ToggleSelection(_state.UpdateItemIds, itemId, selected);
    }

    private void ToggleUpdateSelection(int itemId, ChangeEventArgs args)
    {
        ToggleUpdateSelection(itemId, ParseCheckboxValue(args));
    }

    private static bool ParseCheckboxValue(ChangeEventArgs args)
    {
        return args.Value is true || (args.Value is string value && bool.TryParse(value, out bool parsed) && parsed);
    }

    private static void ToggleSelection(HashSet<int> target, int itemId, bool selected)
    {
        if (selected)
            target.Add(itemId);
        else
            target.Remove(itemId);
    }

    private async Task CreateOrderAsync()
    {
        if (_state.CreateItemIds.Count == 0)
        {
            _state.SetError("Select at least one item.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            GoodHamburgerApiClient.OrderResponse newOrder = await ApiClient.CreateOrderAsync(_state.CreateItemIds.ToList());
            _state.CreateItemIds.Clear();

            if (newOrder != null)
                _state.Orders = _state.Orders.Append(newOrder).ToList();

            _state.SetSuccess("Order created successfully.");
        });
    }

    private async Task LoadOrderForEditAsync()
    {
        if (_state.UpdateOrderId <= 0)
        {
            _state.SetError("Invalid order ID.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            GoodHamburgerApiClient.OrderResponse? order = await ApiClient.GetOrderByIdAsync(_state.UpdateOrderId);
            if (order is null)
            {
                _state.SetError("Order not found.");
                return;
            }

            _state.UpdateItemIds = order.Items.Select(i => i.Id).ToHashSet();
            _state.SetSuccess($"Order {order.Id} loaded.");
        });
    }

    private async Task UpdateOrderAsync()
    {
        if (_state.UpdateOrderId <= 0)
        {
            _state.SetError("Invalid order ID.");
            return;
        }

        if (_state.UpdateItemIds.Count == 0)
        {
            _state.SetError("Select at least one item.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            await ApiClient.UpdateOrderAsync(_state.UpdateOrderId, _state.UpdateItemIds.ToList());

            var index = _state.Orders.ToList().FindIndex(o => o.Id == _state.UpdateOrderId);
            if (index >= 0)
            {
                var updated = await ApiClient.GetOrderByIdAsync(_state.UpdateOrderId);
                if (updated != null)
                {
                    var list = _state.Orders.ToList();
                    list[index] = updated;
                    _state.Orders = list;
                }
            }

            _state.SetSuccess("Order updated successfully.");
        });
    }

    private void StartEdit(GoodHamburgerApiClient.OrderResponse order)
    {
        _state.UpdateOrderId = order.Id;
        _state.UpdateItemIds = order.Items.Select(i => i.Id).ToHashSet();
        _state.SetSuccess($"Order {order.Id} selected.");
    }

    private async Task DeleteOrderAsync(int orderId)
    {
        bool confirmed = await JS.InvokeAsync<bool>("confirm", $"Delete order #{orderId}?");
        if (!confirmed) return;

        await ExecuteAsync(async () =>
        {
            await ApiClient.DeleteOrderAsync(orderId);
            _state.Orders = _state.Orders.Where(o => o.Id != orderId).ToList();
            _state.SetSuccess("Order deleted.");
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        _state.IsBusy = true;
        _state.ClearMessages();

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _state.SetError(MapError(ex));
        }
        finally
        {
            _state.IsBusy = false;
            StateHasChanged();
        }
    }

    private static string MapError(Exception ex)
    {
        string msg = ex.Message.ToLower();

        if (msg.Contains("not found"))
            return "Resource not found.";

        if (msg.Contains("duplicate"))
            return "Duplicate items are not allowed.";

        if (msg.Contains("exactly one sandwich"))
            return "Order must contain exactly one sandwich.";

        return "Unexpected error occurred.";
    }

    private static string FormatCurrency(decimal value)
        => value.ToString("C", new CultureInfo("pt-BR"));

    private static string FormatDateTime(DateTimeOffset value)
        => value.LocalDateTime.ToString("dd/MM/yyyy HH:mm");

    private static string FormatNullableDateTime(DateTimeOffset? value)
        => value.HasValue ? FormatDateTime(value.Value) : "-";
}
