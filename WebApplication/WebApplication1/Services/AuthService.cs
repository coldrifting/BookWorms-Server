using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApplication1.Models.Data;
using WebApplication1.Models.Entities;

namespace WebApplication1.Services;

public static class AuthService
{
    // Generates a new secret JWT signing key each time the server is run
    // This simplifies things, and means that no secret keys will be accidentally uploaded,
    // but it also means that any signed-in users will need to re-sign-in if the server is reset
    private static readonly string Secret = Utils.Hash.GenerateHash();
    public static byte[] SecretBytes => Encoding.UTF8.GetBytes(Secret);
    
    // Expire time of generated JWT tokens in minutes
    public const int ExpireTime = 15; 

    public static string GenerateToken(User user)
    {
        long expTimestamp = DateTime.UtcNow.AddMinutes(ExpireTime).ToFileTimeUtc();
        TokenHeaderDTO headerDTO = new();
        TokenPayloadDTO payloadDTO = new("server", user.Username, expTimestamp, user.Roles);

        JsonSerializerOptions serializeOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string header = JsonSerializer.Serialize(headerDTO, serializeOptions);
        string payload = JsonSerializer.Serialize(payloadDTO, serializeOptions);
        string signature = GenerateJWTSignature(header, payload);

        return Utils.Hash.Base64Encode(header) + '.' + 
               Utils.Hash.Base64Encode(payload) + '.' + 
               signature;
    }
    
    // Generates a base64url encoded signature based on two strings
    private static string GenerateJWTSignature(string header, string payload)
    {
        string encodedHeader = Utils.Hash.Base64Encode(header);
        string encodedPayload = Utils.Hash.Base64Encode(payload);

        var key = Encoding.UTF8.GetBytes(Secret);
        var source = Encoding.UTF8.GetBytes(encodedHeader + '.' + encodedPayload);

        return Utils.Hash.Base64EncodeBytes(HMACSHA256.HashData(key, source));
    }
    
    // Customize error response body on 401 Unauthorized and 403 Forbidden
    public static JwtBearerEvents BearerEvents()
    {
	    return new()
	    {
		    OnChallenge = context =>
		    {
			    context.Response.OnStarting(async () =>
			    {
				    context.Response.Headers.Append("content-type", "application/json; charset=utf-8");
				    ErrorDTO error = new("Unauthenticated", "Valid token required for this route");
				    await context.Response.WriteAsync(JsonSerializer.Serialize(error));
			    });

			    return Task.CompletedTask;
		    },
		    OnForbidden = context =>
		    {
			    context.Response.OnStarting(async () =>
			    {
				    context.Response.Headers.Append("content-type", "application/json; charset=utf-8");
				    ErrorDTO error = new("Unauthorized", "User is not authorized to perform this action");
				    await context.Response.WriteAsync(JsonSerializer.Serialize(error));
			    });

			    return Task.CompletedTask;
		    }
	    };
    }
    
    //
    // Password generation / verification
    //
    
    public static bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        byte[] hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeySize);
        return CryptographicOperations.FixedTimeEquals(hashToCompare, hash);
    }

    private const int KeySize = 64;
    private const int Iterations = 350000;
    public static byte[] HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(KeySize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            KeySize);
         return hash;
    }
}