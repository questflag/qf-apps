using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

/// <summary>
/// Invites a new user by creating an inactive account and sending a verification email
/// with a link to confirm their address and set their initial password.
/// </summary>
public record InviteUserCommand(
    Guid TenantId,
    string Username,
    string Email,
    string DisplayName,
    string RoleName,
    string BaseUrl   // e.g. "https://passport.questflag.com"
) : IRequest<Guid>;

public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEmailSender _emailSender;

    public InviteUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITenantRepository tenantRepository,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _tenantRepository = tenantRepository;
        _emailSender = emailSender;
    }

    public async Task<Guid> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Tenant {request.TenantId} not found.");

        var user = new ApplicationUser
        {
            TenantId = request.TenantId,
            UserName = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName,
            IsActive = false,   // activated only after email verification
            EmailConfirmed = false
        };

        // Create user without a password — they set it after email verification
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await _userManager.AddToRoleAsync(user, request.RoleName);

        // Generate email confirmation token + encode it
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var verifyLink = $"{request.BaseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

        var body = $"""
            <h2>Welcome to QuestFlag — {tenant.Name}</h2>
            <p>You have been invited to join <strong>{tenant.Name}</strong>.</p>
            <p>Click the link below to verify your email and set your password:</p>
            <p><a href="{verifyLink}" style="padding:10px 20px;background:#4f46e5;color:white;border-radius:6px;text-decoration:none;">Accept Invitation</a></p>
            <p>This link expires in 24 hours.</p>
            """;

        await _emailSender.SendAsync(request.Email, $"You're invited to {tenant.Name}", body, cancellationToken);
        return user.Id;
    }
}
