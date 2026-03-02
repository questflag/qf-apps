using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Users.Queries;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PassportAdmin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all users across all tenants (Passport Admin only).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(searchTerm));
        return Ok(result);
    }
}
