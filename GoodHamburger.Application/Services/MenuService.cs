using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Contracts;

namespace GoodHamburger.Application.Services;

public sealed class MenuService(IMenuCatalog menuCatalog) : IMenuService
{
    public IReadOnlyCollection<MenuItemDto> GetMenu()
    {
        return menuCatalog
            .GetAll()
            .Select(item => new MenuItemDto(item.Id, item.Name, item.Category.ToString(), item.Price))
            .ToList();
    }
}
