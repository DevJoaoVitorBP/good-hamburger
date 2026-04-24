using System.Net;
using System.Net.Http.Json;
using GoodHamburger.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GoodHamburger.API.IntegrationTests;

public sealed class OrdersEndpointsIntegrationTests
{
    private const string CorrelationHeader = "X-Correlation-ID";

    public OrdersEndpointsIntegrationTests()
    {
        Factory = new GoodHamburgerApiFactory();
        Client = Factory.CreateApiClient();
    }

    private GoodHamburgerApiFactory Factory { get; }
    private HttpClient Client { get; }

    [Fact]
    public async Task GetOrders_WhenThereAreNoOrders_ShouldReturnOkWithEmptyList()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationHeader));

        IReadOnlyCollection<OrderResponse>? orders = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<OrderResponse>>();
        Assert.NotNull(orders);
        Assert.Empty(orders);
    }

    [Fact]
    public async Task GetMenu_ShouldReturnConfiguredItems()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/menu");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        IReadOnlyCollection<MenuItemResponse>? menu = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<MenuItemResponse>>();
        Assert.NotNull(menu);
        Assert.Equal(5, menu.Count);
        Assert.Contains(menu, item => item.Id == 1 && item.Name == "X Burger");
    }

    [Fact]
    public async Task PostOrder_ThenGetById_ShouldReturnCreatedOrder()
    {
        HttpResponseMessage createResponse = await Client.PostAsJsonAsync("/api/orders", new CreateOrderRequest([1, 4, 5]));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        OrderResponse? created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);
        Assert.Equal(0.20m, created.DiscountPercentage);
        Assert.Equal(7.60m, created.Total);

        HttpResponseMessage getByIdResponse = await Client.GetAsync($"/api/orders/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        OrderResponse? loaded = await getByIdResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded.Id);
        Assert.Equal(created.Total, loaded.Total);
    }

    [Fact]
    public async Task PutOrder_ShouldUpdateTotals()
    {
        HttpResponseMessage createResponse = await Client.PostAsJsonAsync("/api/orders", new CreateOrderRequest([1, 4, 5]));
        OrderResponse? created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(created);

        HttpResponseMessage updateResponse = await Client.PutAsJsonAsync($"/api/orders/{created.Id}", new UpdateOrderRequest([1, 5]));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        OrderResponse? updated = await updateResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(updated);
        Assert.Equal(0.15m, updated.DiscountPercentage);
        Assert.Equal(6.37m, updated.Total);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturnNoContent_AndGetByIdShouldReturnNotFound()
    {

        HttpResponseMessage createResponse = await Client.PostAsJsonAsync("/api/orders", new CreateOrderRequest([1, 4]));
        OrderResponse? created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await Client.DeleteAsync($"/api/orders/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getByIdResponse = await Client.GetAsync($"/api/orders/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getByIdResponse.StatusCode);
    }

    [Fact]
    public async Task PostOrder_WithDuplicatedItems_ShouldReturnBadRequest()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/orders", new CreateOrderRequest([1, 1]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationHeader));

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Duplicate", problem.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.True(problem.Extensions.ContainsKey("correlationId"));
    }

    [Fact]
    public async Task RequestWithCorrelationIdHeader_ShouldEchoSameCorrelationId()
    {
        const string requestedCorrelationId = "test-correlation-id-123";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/menu");
        request.Headers.Add(CorrelationHeader, requestedCorrelationId);

        HttpResponseMessage response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues(CorrelationHeader, out IEnumerable<string>? values));
        Assert.Contains(requestedCorrelationId, values);
    }

    private sealed class GoodHamburgerApiFactory : WebApplicationFactory<Program>
    {
        public HttpClient CreateApiClient()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        }
    }

    private sealed record CreateOrderRequest(IReadOnlyCollection<int> ItemIds);
    private sealed record UpdateOrderRequest(IReadOnlyCollection<int> ItemIds);

    private sealed record MenuItemResponse(int Id, string Name, string Category, decimal Price);

    private sealed record OrderItemResponse(int Id, string Name, string Category, decimal Price);

    private sealed record OrderResponse(
        int Id,
        IReadOnlyCollection<OrderItemResponse> Items,
        decimal Subtotal,
        decimal DiscountPercentage,
        decimal DiscountAmount,
        decimal Total,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);
}
