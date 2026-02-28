using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestFlag.Passport.Application.Features.Users.Commands;

namespace QuestFlag.Passport.Services.Controllers;

/// <summary>
/// Handles user account lifecycle: email verification, password reset, 2FA setup, profile.
/// Anonymous endpoints are accessible without authentication.
/// Authenticated endpoints require a valid JWT from the caller.
/// </summary>
[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public AccountController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config = config;
    }

    // ─────────────────────────────────────────────────────────
    // Anonymous endpoints
    // ─────────────────────────────────────────────────────────

    /// <summary>Sends a password-reset link to the given email address.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var baseUrl = _config["Passport:WebAppBaseUrl"] ?? "https://localhost:7003";
        await _mediator.Send(new SendPasswordResetEmailCommand(request.Email, baseUrl));
        // Always return OK — never reveal whether the email exists
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    /// <summary>Resets the user's password using the token from the email.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _mediator.Send(new ResetPasswordCommand(request.UserId, request.Token, request.NewPassword));
        return success ? Ok(new { message = "Password reset successfully." }) : BadRequest(new { error = "Invalid or expired token." });
    }

    /// <summary>
    /// Verifies the email confirmation token from an invite email and sets the user's initial password.
    /// Activates the account on success.
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var success = await _mediator.Send(new VerifyEmailAndSetPasswordCommand(request.UserId, request.Token, request.Password));
        return success ? Ok(new { message = "Email verified and password set. You can now log in." })
                       : BadRequest(new { error = "Invalid or expired verification link." });
    }

    // ─────────────────────────────────────────────────────────
    // Authenticated endpoints (require valid JWT)
    // ─────────────────────────────────────────────────────────

    /// <summary>Starts phone 2FA enrollment — sends an OTP to the given number.</summary>
    [HttpPost("two-factor/enable-phone")]
    [Authorize]
    public async Task<IActionResult> EnablePhoneTwoFactor([FromBody] EnablePhoneRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _mediator.Send(new EnablePhoneTwoFactorCommand(userId.Value, request.PhoneNumber));
        return Ok(new { message = "OTP sent to the provided phone number." });
    }

    /// <summary>Confirms the OTP and enables 2FA on the account.</summary>
    [HttpPost("two-factor/verify-phone")]
    [Authorize]
    public async Task<IActionResult> VerifyPhoneTwoFactor([FromBody] VerifyPhoneRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _mediator.Send(new ConfirmPhoneTwoFactorCommand(userId.Value, request.PhoneNumber, request.Otp));
        return success ? Ok(new { message = "2FA enabled successfully." }) : BadRequest(new { error = "Invalid OTP." });
    }

    /// <summary>Sends the login-time 2FA OTP (called by the SSO login page after password validation).</summary>
    [HttpPost("two-factor/send-login-otp")]
    [AllowAnonymous] // User is mid-login; not yet fully authenticated
    public async Task<IActionResult> SendLoginOtp([FromBody] SendLoginOtpRequest request)
    {
        await _mediator.Send(new SendLoginOtpCommand(request.UserId));
        return Ok(new { message = "OTP sent." });
    }

    /// <summary>Verifies the login-time OTP. On success the SSO flow can issue the auth code.</summary>
    [HttpPost("two-factor/verify-login-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyLoginOtpRequest request)
    {
        var valid = await _mediator.Send(new VerifyLoginOtpCommand(request.UserId, request.Otp));
        return valid ? Ok(new { verified = true }) : BadRequest(new { error = "Invalid or expired OTP." });
    }

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirst(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject)?.Value
               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);
public record VerifyEmailRequest(Guid UserId, string Token, string Password);
public record EnablePhoneRequest(string PhoneNumber);
public record VerifyPhoneRequest(string PhoneNumber, string Otp);
public record SendLoginOtpRequest(Guid UserId);
public record VerifyLoginOtpRequest(Guid UserId, string Otp);
