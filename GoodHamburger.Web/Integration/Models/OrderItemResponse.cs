namespace GoodHamburger.Web.Integration.Models;

public sealed record OrderItemResponse(int Id, string Name, string Category, decimal Price);