using System.Security.Cryptography;
using System.Text;

namespace QuestFlag.Demo.WebApp.Client.Helpers;

public static class PkceHelper
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01233456789-._~";

    public static string GenerateCodeVerifier(int length = 128)
    {
        if (length < 43 || length > 128)
            throw new ArgumentOutOfRangeException(nameof(length), "PKCE code_verifier must be between 43 and 128 characters.");

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var result = new StringBuilder(length);
        foreach (var b in bytes)
        {
            result.Append(Chars[b % Chars.Length]);
        }

        return result.ToString();
    }

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.ASCII.GetBytes(codeVerifier);
        var hash = sha256.ComputeHash(bytes);
        
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
