using GoodHamburger.Application.Contracts;
using GoodHamburger.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoodHamburger.API.Controllers;

[ApiController]
[Route("api/menu")]
public sealed class MenuController(IMenuService menuService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<MenuItemDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<MenuItemDto>> Get()
    {
        return Ok(menuService.GetMenu());
    }
}
