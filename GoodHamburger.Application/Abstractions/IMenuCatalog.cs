using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Abstractions;

public interface IMenuCatalog
{
    IReadOnlyCollection<MenuItem> GetAll();
    MenuItem? GetById(int id);
}
