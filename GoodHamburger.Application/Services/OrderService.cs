using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Exceptions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Services;

public sealed class OrderService(IOrderRepository orderRepository, IMenuCatalog menuCatalog) : IOrderService
{
    public async Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Order> orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ResourceNotFoundException($"Order '{id}' was not found.");
        }

        return Map(order);
    }

    public async Task<OrderDto> CreateAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<MenuItem> items = ValidateAndResolveItems(itemIds);

        decimal subtotal, discountPercentage, discountAmount, total;
        (subtotal, discountPercentage, discountAmount, total) = CalculateTotals(items);

        int nextId = await orderRepository.GetNextIdAsync(cancellationToken);
        Order order = Order.Create(nextId, items, subtotal, discountPercentage, discountAmount, total);
        await orderRepository.AddAsync(order, cancellationToken);

        return Map(order);
    }

    public async Task<OrderDto> UpdateAsync(int id, IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ResourceNotFoundException($"Order '{id}' was not found.");
        }

        IReadOnlyList<MenuItem> items = ValidateAndResolveItems(itemIds);

        decimal subtotal, discountPercentage, discountAmount, total;
        (subtotal, discountPercentage, discountAmount, total) = CalculateTotals(items);

        order.Update(items, subtotal, discountPercentage, discountAmount, total);
        await orderRepository.UpdateAsync(order, cancellationToken);

        return Map(order);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        bool removed = await orderRepository.DeleteAsync(id, cancellationToken);
        if (!removed)
        {
            throw new ResourceNotFoundException($"Order '{id}' was not found.");
        }
    }

    private IReadOnlyList<MenuItem> ValidateAndResolveItems(IReadOnlyCollection<int> itemIds)
    {
        if (itemIds.Count == 0)
        {
            throw new BusinessRuleValidationException("Order must contain at least one item.");
        }

        IReadOnlyList<int> duplicatedItems = itemIds
            .GroupBy(id => id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicatedItems.Count > 0)
        {
            throw new BusinessRuleValidationException($"Duplicate items are not allowed: {string.Join(", ", duplicatedItems)}.");
        }

        List<MenuItem> items = new List<MenuItem>();
        List<int> notFoundItems = new List<int>();

        foreach (int itemId in itemIds)
        {
            MenuItem? menuItem = menuCatalog.GetById(itemId);
            if (menuItem is null)
            {
                notFoundItems.Add(itemId);
                continue;
            }

            items.Add(menuItem);
        }

        if (notFoundItems.Count > 0)
        {
            throw new BusinessRuleValidationException($"Items were not found in the menu: {string.Join(", ", notFoundItems)}.");
        }

        ValidateCategoryRules(items);

        return items;
    }

    private static void ValidateCategoryRules(IReadOnlyCollection<MenuItem> items)
    {
        int sandwichCount = items.Count(item => item.Category == MenuCategory.Sandwich);
        int friesCount = items.Count(item => item.Category == MenuCategory.Fries);
        int drinkCount = items.Count(item => item.Category == MenuCategory.Drink);

        if (sandwichCount != 1)
        {
            throw new BusinessRuleValidationException("Order must contain exactly one sandwich.");
        }

        if (friesCount > 1)
        {
            throw new BusinessRuleValidationException("Order can contain at most one fries.");
        }

        if (drinkCount > 1)
        {
            throw new BusinessRuleValidationException("Order can contain at most one soft drink.");
        }
    }

    private static (decimal Subtotal, decimal DiscountPercentage, decimal DiscountAmount, decimal Total) CalculateTotals(IReadOnlyCollection<MenuItem> items)
    {
        decimal subtotal = items.Sum(item => item.Price);

        bool hasSandwich = items.Any(item => item.Category == MenuCategory.Sandwich);
        bool hasFries = items.Any(item => item.Category == MenuCategory.Fries);
        bool hasDrink = items.Any(item => item.Category == MenuCategory.Drink);

        decimal discountPercentage = 0m;

        if (hasSandwich && hasFries && hasDrink)
        {
            discountPercentage = 0.20m;
        }
        else if (hasSandwich && hasDrink)
        {
            discountPercentage = 0.15m;
        }
        else if (hasSandwich && hasFries)
        {
            discountPercentage = 0.10m;
        }

        decimal discountAmount = Math.Round(subtotal * discountPercentage, 2, MidpointRounding.AwayFromZero);
        decimal total = Math.Round(subtotal - discountAmount, 2, MidpointRounding.AwayFromZero);

        return (subtotal, discountPercentage, discountAmount, total);
    }

    private static OrderDto Map(Order order)
    {
        List<OrderItemDto> items = order.Items
            .Select(item => new OrderItemDto(item.Id, item.Name, item.Category.ToString(), item.Price))
            .ToList();

        return new OrderDto(
            order.Id,
            items,
            order.Subtotal,
            order.DiscountPercentage,
            order.DiscountAmount,
            order.Total,
            order.CreatedAt,
            order?.UpdatedAt);
    }
}
