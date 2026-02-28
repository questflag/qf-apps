using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

/// <summary>
/// Verifies the email confirmation token and sets the user's initial password.
/// Activates the account on success.
/// </summary>
public record VerifyEmailAndSetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword
) : IRequest<bool>;

public class VerifyEmailAndSetPasswordCommandHandler : IRequestHandler<VerifyEmailAndSetPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyEmailAndSetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<bool> Handle(VerifyEmailAndSetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return false;

        // Confirm email
        var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!confirmResult.Succeeded) return false;

        // Set password
        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
            throw new InvalidOperationException($"Failed to set password: {string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))}");

        // Activate account
        user.IsActive = true;
        await _userManager.UpdateAsync(user);
        return true;
    }
}
