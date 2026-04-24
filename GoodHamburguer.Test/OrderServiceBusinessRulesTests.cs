using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Exceptions;
using GoodHamburger.Application.Services;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Xunit;

namespace GoodHamburguer.Test;

public sealed class OrderServiceBusinessRulesTests
{
    private readonly OrderService _orderService;

    public OrderServiceBusinessRulesTests()
    {
        _orderService = new OrderService(new InMemoryOrderRepositoryFake(), new MenuCatalogFake());
    }

    [Fact]
    public async Task CreateAsync_WhenOrderHasSandwichFriesAndDrink_ShouldApplyTwentyPercentDiscount()
    {
        OrderDto result = await _orderService.CreateAsync([1, 4, 5]);

        Assert.Equal(9.50m, result.Subtotal);
        Assert.Equal(0.20m, result.DiscountPercentage);
        Assert.Equal(1.90m, result.DiscountAmount);
        Assert.Equal(7.60m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_WhenOrderHasSandwichAndDrink_ShouldApplyFifteenPercentDiscount()
    {
        OrderDto result = await _orderService.CreateAsync([1, 5]);

        Assert.Equal(7.50m, result.Subtotal);
        Assert.Equal(0.15m, result.DiscountPercentage);
        Assert.Equal(1.13m, result.DiscountAmount);
        Assert.Equal(6.37m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_WhenOrderHasSandwichAndFries_ShouldApplyTenPercentDiscount()
    {
        OrderDto result = await _orderService.CreateAsync([1, 4]);

        Assert.Equal(7.00m, result.Subtotal);
        Assert.Equal(0.10m, result.DiscountPercentage);
        Assert.Equal(0.70m, result.DiscountAmount);
        Assert.Equal(6.30m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_WhenItemsAreDuplicated_ShouldThrowBusinessRuleValidationException()
    {
        BusinessRuleValidationException exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() => _orderService.CreateAsync([1, 1]));

        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenItemDoesNotExistInMenu_ShouldThrowBusinessRuleValidationException()
    {
        BusinessRuleValidationException exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() => _orderService.CreateAsync([1, 99]));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WhenOrderHasNoSandwich_ShouldThrowBusinessRuleValidationException()
    {
        BusinessRuleValidationException exception = await Assert.ThrowsAsync<BusinessRuleValidationException>(() => _orderService.CreateAsync([4, 5]));

        Assert.Contains("exactly one sandwich", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class MenuCatalogFake : IMenuCatalog
    {
        private static readonly IReadOnlyCollection<MenuItem> Items =
        [
            new(1, "X Burger", MenuCategory.Sandwich, 5.00m),
            new(2, "X Egg", MenuCategory.Sandwich, 4.50m),
            new(3, "X Bacon", MenuCategory.Sandwich, 7.00m),
            new(4, "French fries", MenuCategory.Fries, 2.00m),
            new(5, "Soda", MenuCategory.Drink, 2.50m)
        ];

        public IReadOnlyCollection<MenuItem> GetAll() => Items;

        public MenuItem? GetById(int id)
        {
            return Items.FirstOrDefault(item => item.Id == id);
        }
    }

    private sealed class InMemoryOrderRepositoryFake : IOrderRepository
    {
        private readonly Dictionary<int, Order> _orders = [];
        private int _nextId;

        public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Order>>(_orders.Values.ToList());
        }

        public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _orders.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
        {
            _nextId++;
            return Task.FromResult(_nextId);
        }

        public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_orders.Remove(id));
        }
    }
}
