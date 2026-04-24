using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public sealed record MenuItem(int Id, string Name, MenuCategory Category, decimal Price);
