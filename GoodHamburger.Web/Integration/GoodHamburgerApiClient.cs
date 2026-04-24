using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Web.Integration;

public sealed class GoodHamburgerApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<MenuItemResponse>> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<MenuItemResponse>? result = await httpClient.GetFromJsonAsync<IReadOnlyCollection<MenuItemResponse>>("api/menu", cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyCollection<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/orders", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        await EnsureSuccessAsync(response, cancellationToken);

        IReadOnlyCollection<OrderResponse>? result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<OrderResponse>>(cancellationToken);
        return result ?? [];
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/orders/{orderId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<OrderResponse>(cancellationToken);
    }

    public async Task<OrderResponse> CreateOrderAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/orders", new CreateOrUpdateOrderRequest(itemIds), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        OrderResponse? result = await response.Content.ReadFromJsonAsync<OrderResponse>(cancellationToken);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task<OrderResponse> UpdateOrderAsync(int orderId, IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/orders/{orderId}", new CreateOrUpdateOrderRequest(itemIds), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        OrderResponse? result = await response.Content.ReadFromJsonAsync<OrderResponse>(cancellationToken);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/orders/{orderId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        string message = problemDetails?.Detail ?? problemDetails?.Title ?? $"The API call failed with status code {(int)response.StatusCode}.";

        throw new InvalidOperationException(message);
    }

    public sealed record MenuItemResponse(int Id, string Name, string Category, decimal Price);

    public sealed record OrderItemResponse(int Id, string Name, string Category, decimal Price);

    public sealed record OrderResponse(
        int Id,
        IReadOnlyCollection<OrderItemResponse> Items,
        decimal Subtotal,
        decimal DiscountPercentage,
        decimal DiscountAmount,
        decimal Total,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private sealed record CreateOrUpdateOrderRequest(IReadOnlyCollection<int> ItemIds);
}
