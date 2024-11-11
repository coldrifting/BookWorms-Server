using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Utils;

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

    private static string Base64Decode(string base64)
    {
        return Encoding.UTF8.GetString(Base64DecodeBytes(base64));
    }

    private static byte[] Base64DecodeBytes(string base64EncodedData)
    {
        string base64 = base64EncodedData.Replace('-', '+')
                                         .Replace('_', '/');
        while (base64.Length % 4 != 0)
        {
            base64 += '=';
        }
        
        return Convert.FromBase64String(base64);
    }
}