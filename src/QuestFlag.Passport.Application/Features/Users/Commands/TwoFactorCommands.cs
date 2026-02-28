using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

/// <summary>
/// Initiates phone 2FA enrollment: generates a 6-digit OTP and sends it via SMS.
/// OTP is stored temporarily via Identity's phone change token provider.
/// </summary>
public record EnablePhoneTwoFactorCommand(Guid UserId, string PhoneNumber) : IRequest<bool>;

public class EnablePhoneTwoFactorCommandHandler : IRequestHandler<EnablePhoneTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsSender _smsSender;

    public EnablePhoneTwoFactorCommandHandler(UserManager<ApplicationUser> userManager, ISmsSender smsSender)
    {
        _userManager = userManager;
        _smsSender = smsSender;
    }

    public async Task<bool> Handle(EnablePhoneTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return false;

        // Set the pending phone number (not yet confirmed)
        await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);

        // Generate OTP using Identity's change phone number token provider
        var otp = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);

        await _smsSender.SendAsync(request.PhoneNumber,
            $"Your QuestFlag verification code is: {otp}. Valid for 10 minutes.", cancellationToken);
        return true;
    }
}

/// <summary>
/// Confirms the OTP sent by <see cref="EnablePhoneTwoFactorCommand"/> and enables 2FA.
/// </summary>
public record ConfirmPhoneTwoFactorCommand(Guid UserId, string PhoneNumber, string Otp) : IRequest<bool>;

public class ConfirmPhoneTwoFactorCommandHandler : IRequestHandler<ConfirmPhoneTwoFactorCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmPhoneTwoFactorCommandHandler(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<bool> Handle(ConfirmPhoneTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return false;

        var result = await _userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Otp);
        if (!result.Succeeded) return false;

        // Enable 2FA on the account
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return true;
    }
}

/// <summary>
/// Sends a login-time 2FA OTP to the user's confirmed phone number.
/// </summary>
public record SendLoginOtpCommand(Guid UserId) : IRequest<bool>;

public class SendLoginOtpCommandHandler : IRequestHandler<SendLoginOtpCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsSender _smsSender;

    public SendLoginOtpCommandHandler(UserManager<ApplicationUser> userManager, ISmsSender smsSender)
    {
        _userManager = userManager;
        _smsSender = smsSender;
    }

    public async Task<bool> Handle(SendLoginOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null || string.IsNullOrEmpty(user.PhoneNumber)) return false;

        // Use phone number as token purpose (ties OTP to current phone)
        var otp = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
        await _smsSender.SendAsync(user.PhoneNumber,
            $"Your QuestFlag login code is: {otp}. Valid for 5 minutes.", cancellationToken);
        return true;
    }
}

/// <summary>Verifies the login-time 2FA OTP supplied by the user.</summary>
public record VerifyLoginOtpCommand(Guid UserId, string Otp) : IRequest<bool>;

public class VerifyLoginOtpCommandHandler : IRequestHandler<VerifyLoginOtpCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyLoginOtpCommandHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<bool> Handle(VerifyLoginOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) return false;

        return await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, request.Otp);
    }
}
