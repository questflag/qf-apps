using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Application.Features.Users.Commands;
using QuestFlag.Passport.Application.Features.Users.Queries;

namespace QuestFlag.Passport.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists active trusted devices for the current authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyDevices()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var devices = await _mediator.Send(new GetTrustedDevicesQuery(userId.Value));
        return Ok(devices.Select(d => new
        {
            d.Id,
            d.DeviceName,
            d.IpAddress,
            d.TrustedAtUtc,
            d.ExpiresAtUtc
        }));
    }

    /// <summary>Revokes trust for a specific device.</summary>
    [HttpDelete("{deviceId:guid}")]
    public async Task<IActionResult> RevokeDevice(Guid deviceId)
    {
        var success = await _mediator.Send(new RevokeDeviceCommand(deviceId));
        return success ? Ok(new { message = "Device revoked." }) : NotFound();
    }

    /// <summary>Revokes all trusted devices for the current user.</summary>
    [HttpDelete]
    public async Task<IActionResult> RevokeAllDevices()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _mediator.Send(new RevokeAllDevicesCommand(userId.Value));
        return Ok(new { message = "All devices revoked." });
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
