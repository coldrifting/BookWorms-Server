using System.Security.Cryptography;
using System.Text;

namespace BookwormsServer.Utils;

// Common hash generation functions
public static class Hash
{
    public static string GenerateHash()
    {
        return Convert.ToBase64String(new HMACSHA256().Key);
    }

    public static string Base64Encode(string input)
    {
        return Base64EncodeBytes(Encoding.UTF8.GetBytes(input));
    }

    public static string Base64EncodeBytes(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}