using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

/// <summary>
/// Records a trusted device entry after the user opts to "Remember this device".
/// Returns the raw device token — caller sets this as an HttpOnly cookie.
/// </summary>
public record TrustDeviceCommand(
    Guid UserId,
    string DeviceName,
    string IpAddress,
    int ExpiryDays = 30
) : IRequest<string>; // Returns raw token

public class TrustDeviceCommandHandler : IRequestHandler<TrustDeviceCommand, string>
{
    private readonly ITrustedDeviceRepository _deviceRepo;

    public TrustDeviceCommandHandler(ITrustedDeviceRepository deviceRepo) => _deviceRepo = deviceRepo;

    public async Task<string> Handle(TrustDeviceCommand request, CancellationToken cancellationToken)
    {
        // Generate a 64-byte cryptographically-random token
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        // Hash it for storage — raw token lives only in the browser cookie
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var device = new TrustedDevice
        {
            UserId = request.UserId,
            DeviceTokenHash = tokenHash,
            DeviceName = request.DeviceName,
            IpAddress = request.IpAddress,
            TrustedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(request.ExpiryDays)
        };

        await _deviceRepo.AddAsync(device, cancellationToken);
        return rawToken; // caller sets this as HttpOnly cookie
    }
}

/// <summary>Revokes a specific trusted device by ID.</summary>
public record RevokeDeviceCommand(Guid DeviceId) : IRequest<bool>;

public class RevokeDeviceCommandHandler : IRequestHandler<RevokeDeviceCommand, bool>
{
    private readonly ITrustedDeviceRepository _deviceRepo;

    public RevokeDeviceCommandHandler(ITrustedDeviceRepository deviceRepo) => _deviceRepo = deviceRepo;

    public async Task<bool> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device == null) return false;

        device.IsRevoked = true;
        await _deviceRepo.UpdateAsync(device, cancellationToken);
        return true;
    }
}

/// <summary>Revokes all trusted devices for a user (e.g. on password change or admin action).</summary>
public record RevokeAllDevicesCommand(Guid UserId) : IRequest;

public class RevokeAllDevicesCommandHandler : IRequestHandler<RevokeAllDevicesCommand>
{
    private readonly ITrustedDeviceRepository _deviceRepo;

    public RevokeAllDevicesCommandHandler(ITrustedDeviceRepository deviceRepo) => _deviceRepo = deviceRepo;

    public async Task Handle(RevokeAllDevicesCommand request, CancellationToken cancellationToken)
        => await _deviceRepo.RevokeAllForUserAsync(request.UserId, cancellationToken);
}
