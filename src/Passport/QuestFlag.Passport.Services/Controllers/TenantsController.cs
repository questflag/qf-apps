using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Tenants.Commands;
using QuestFlag.Passport.Application.Features.Tenants.Queries;
using QuestFlag.Passport.Application.Features.Users.Commands;
using QuestFlag.Passport.Application.Features.Users.Queries;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantRepository _tenantRepo;
    private readonly IConfiguration _config;

    public TenantsController(IMediator mediator, ITenantRepository tenantRepo, IConfiguration config)
    {
        _mediator = mediator;
        _tenantRepo = tenantRepo;
        _config = config;
    }

    /// <summary>
    /// Lists active tenants (public — used by SSO login page tenant dropdown).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTenants()
    {
        var result = await _mediator.Send(new GetTenantsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Resolves a tenant from a host/domain value (used by Passport.WebApp SSO page).
    /// GET /api/tenants/resolve?domain=acme.questflag.com
    /// </summary>
    [HttpGet("resolve")]
    [AllowAnonymous]
    public async Task<IActionResult> ResolveByDomain([FromQuery] string domain)
    {
        if (string.IsNullOrWhiteSpace(domain)) return BadRequest();
        var tenant = await _tenantRepo.GetByDomainAsync(domain);
        return tenant != null ? Ok(new { tenant.Id, tenant.Name, tenant.Slug }) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = "PassportAdmin")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpGet("{tenantId}/users")]
    [Authorize]
    public async Task<IActionResult> GetUsers(System.Guid tenantId)
    {
        var result = await _mediator.Send(new GetUsersByTenantQuery(tenantId));
        return Ok(result);
    }

    [HttpPost("{tenantId}/users")]
    [Authorize(Policy = "PassportAdmin")]
    public async Task<IActionResult> CreateUser(System.Guid tenantId, [FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(
            tenantId,
            request.Username,
            request.Email,
            request.Password,
            request.DisplayName,
            request.RoleName);

        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    /// <summary>
    /// Invites a new user: creates inactive account + sends verification email.
    /// </summary>
    [HttpPost("{tenantId}/users/invite")]
    [Authorize(Policy = "PassportAdmin")]
    public async Task<IActionResult> InviteUser(System.Guid tenantId, [FromBody] InviteUserRequest request)
    {
        var baseUrl = _config["Passport:WebAppBaseUrl"] ?? "https://localhost:7003";
        var id = await _mediator.Send(new InviteUserCommand(
            tenantId,
            request.Username,
            request.Email,
            request.DisplayName,
            request.RoleName,
            baseUrl));

        return Ok(new { id });
    }
}

public record CreateUserRequest(string Username, string Email, string Password, string DisplayName, string RoleName);
public record InviteUserRequest(string Username, string Email, string DisplayName, string RoleName);

