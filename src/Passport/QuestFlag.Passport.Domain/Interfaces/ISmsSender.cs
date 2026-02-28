using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.Domain.Interfaces;

/// <summary>
/// Abstraction over an SMS provider (Twilio, AWS SNS, etc.).
/// Used for phone-based 2FA OTP delivery.
/// </summary>
public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, string messageBody, CancellationToken ct = default);
}
