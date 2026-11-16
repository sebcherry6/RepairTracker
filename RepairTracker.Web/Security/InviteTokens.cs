using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace RepairTracker.Web.Security;

public static class InviteTokens
{
    // 32 random bytes = 256 bits. Guessing one is as hopeless as guessing an encryption key.
    // Base64Url keeps it safe to drop straight into a link (no + / or =).
    public static string Create() =>
        WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    // What goes in the database. The same token always gives the same hash,
    // so I can look an invite up by it without ever storing the token itself.
    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}