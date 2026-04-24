using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Infrastructure.Catalog;

public sealed class StaticMenuCatalog : IMenuCatalog
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
