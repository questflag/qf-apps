using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Application.Features.Auth.Commands;
using QuestFlag.Passport.Application.Features.Users.Commands;
using QuestFlag.Passport.Application.Features.Users.Queries;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PassportAdmin")]
public class UserSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserSessionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Admin: lists active trusted devices for a given user.
    /// (Active OpenIddict tokens are managed via IOpenIddictTokenManager internally.)
    /// </summary>
    [HttpGet("{userId:guid}/devices")]
    public async Task<IActionResult> GetUserDevices(Guid userId)
    {
        var devices = await _mediator.Send(new GetTrustedDevicesQuery(userId));
        return Ok(devices.Select(d => new
        {
            d.Id,
            d.DeviceName,
            d.IpAddress,
            d.TrustedAtUtc,
            d.ExpiresAtUtc
        }));
    }

    /// <summary>Admin: force logout — revokes all tokens + updates security stamp for user.</summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> ForceLogout(Guid userId)
    {
        await _mediator.Send(new RevokeUserSessionsCommand(userId));
        await _mediator.Send(new RevokeAllDevicesCommand(userId));
        return Ok(new { message = "User has been force-logged out and all devices revoked." });
    }

    /// <summary>Admin: revoke a specific trusted device for any user.</summary>
    [HttpDelete("devices/{deviceId:guid}")]
    public async Task<IActionResult> RevokeUserDevice(Guid deviceId)
    {
        var success = await _mediator.Send(new RevokeDeviceCommand(deviceId));
        return success ? Ok(new { message = "Device revoked." }) : NotFound();
    }
}
