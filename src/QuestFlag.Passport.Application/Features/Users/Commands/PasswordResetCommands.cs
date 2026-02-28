using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

/// <summary>Sends a password-reset link to the given email address.</summary>
public record SendPasswordResetEmailCommand(string Email, string BaseUrl) : IRequest<bool>;

public class SendPasswordResetEmailCommandHandler : IRequestHandler<SendPasswordResetEmailCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public SendPasswordResetEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<bool> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        // Don't reveal whether the user exists (security best practice)
        if (user == null || !user.IsActive) return true;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var resetLink = $"{request.BaseUrl}/reset-password?userId={user.Id}&token={encodedToken}";

        var body = $"""
            <h2>QuestFlag — Password Reset</h2>
            <p>We received a request to reset your password.</p>
            <p><a href="{resetLink}" style="padding:10px 20px;background:#4f46e5;color:white;border-radius:6px;text-decoration:none;">Reset Password</a></p>
            <p>If you didn't request this, you can safely ignore this email.</p>
            <p>This link expires in 1 hour.</p>
            """;

        await _emailSender.SendAsync(request.Email, "QuestFlag — Reset your password", body, cancellationToken);
        return true;
    }
}

/// <summary>Completes the password reset using the token from the email.</summary>
public record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return false;

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded;
    }
}
