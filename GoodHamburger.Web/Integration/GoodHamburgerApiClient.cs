using System.Net;
using GoodHamburger.Web.Integration.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.Web.Integration;

public sealed class GoodHamburgerApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyCollection<MenuItemResponse>> GetMenuAsync(CancellationToken ct = default)
    {
        return await GetCollectionAsync<MenuItemResponse>("api/menu", ct);
    }

    public async Task<IReadOnlyCollection<OrderResponse>> GetOrdersAsync(CancellationToken ct = default)
    {
        return await GetCollectionAsync<OrderResponse>("api/orders", ct);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        return await GetAsync<OrderResponse>($"api/orders/{orderId}", ct);
    }

    public async Task<OrderResponse> CreateOrderAsync(IReadOnlyCollection<int> itemIds, CancellationToken ct = default)
    {
        return await SendAsync<OrderResponse>(
            HttpMethod.Post,
            "api/orders",
            new CreateOrUpdateOrderRequest(itemIds),
            ct);
    }

    public async Task<OrderResponse> UpdateOrderAsync(int orderId, IReadOnlyCollection<int> itemIds, CancellationToken ct = default)
    {
        return await SendAsync<OrderResponse>(
            HttpMethod.Put,
            $"api/orders/{orderId}",
            new CreateOrUpdateOrderRequest(itemIds),
            ct);
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken ct = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/orders/{orderId}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task<IReadOnlyCollection<T>> GetCollectionAsync<T>(string url, CancellationToken ct)
    {
        IReadOnlyCollection<T>? result = await GetAsync<IReadOnlyCollection<T>>(url, ct);
        return result ?? Array.Empty<T>();
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        await EnsureSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object body, CancellationToken ct)
    {
        using HttpRequestMessage request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body)
        };

        HttpResponseMessage response = await httpClient.SendAsync(request, ct);

        await EnsureSuccessAsync(response, ct);

        T? result = await response.Content.ReadFromJsonAsync<T>(ct);

        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message;

        try
        {
            ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            message = problem?.Detail
                      ?? problem?.Title
                      ?? $"Request failed with status {(int)response.StatusCode}.";
        }
        catch
        {
            message = $"Request failed with status {(int)response.StatusCode}.";
        }

        throw new InvalidOperationException(message);
    }
}