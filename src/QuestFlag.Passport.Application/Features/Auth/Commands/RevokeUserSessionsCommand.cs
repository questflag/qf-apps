using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenIddict.Abstractions;

namespace QuestFlag.Passport.Application.Features.Auth.Commands;

/// <summary>
/// Revokes all active tokens (access + refresh) for a given user.
/// Used by admins for force logout. Changing SecurityStamp also
/// invalidates any future refresh-token grants for this user.
/// </summary>
public record RevokeUserSessionsCommand(Guid UserId) : IRequest;

public class RevokeUserSessionsCommandHandler : IRequestHandler<RevokeUserSessionsCommand>
{
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly IOpenIddictAuthorizationManager _authManager;
    private readonly Microsoft.AspNetCore.Identity.UserManager<QuestFlag.Passport.Domain.Entities.ApplicationUser> _userManager;

    public RevokeUserSessionsCommandHandler(
        IOpenIddictTokenManager tokenManager,
        IOpenIddictAuthorizationManager authManager,
        Microsoft.AspNetCore.Identity.UserManager<QuestFlag.Passport.Domain.Entities.ApplicationUser> userManager)
    {
        _tokenManager = tokenManager;
        _authManager = authManager;
        _userManager = userManager;
    }

    public async Task Handle(RevokeUserSessionsCommand request, CancellationToken cancellationToken)
    {
        var subjectId = request.UserId.ToString();

        // Revoke all authorizations for this subject → cascades to related tokens
        await foreach (var authorization in _authManager.FindBySubjectAsync(subjectId, cancellationToken))
            await _authManager.TryRevokeAsync(authorization, cancellationToken);

        // Revoke remaining orphan tokens
        await foreach (var token in _tokenManager.FindBySubjectAsync(subjectId, cancellationToken))
            await _tokenManager.TryRevokeAsync(token, cancellationToken);

        // Update SecurityStamp — causes any remaining refresh tokens to fail validation
        var user = await _userManager.FindByIdAsync(subjectId);
        if (user != null)
        {
            user.LastLogoutAtUtc = DateTime.UtcNow;
            await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}
