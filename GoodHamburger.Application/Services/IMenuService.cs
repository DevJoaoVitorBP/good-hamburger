using GoodHamburger.Application.Contracts;

namespace GoodHamburger.Application.Services;

public interface IMenuService
{
    IReadOnlyCollection<MenuItemDto> GetMenu();
}
