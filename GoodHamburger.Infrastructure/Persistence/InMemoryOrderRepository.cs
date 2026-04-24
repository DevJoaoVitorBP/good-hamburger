using System.Collections.Concurrent;
using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Infrastructure.Persistence;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<int, Order> _orders = new();
    private int _currentId;

    public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Order> orders = _orders.Values
            .OrderByDescending(order => order.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<Order>>(orders);
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
    {
        int nextId = Interlocked.Increment(ref _currentId);
        return Task.FromResult(nextId);
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
        bool removed = _orders.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}
