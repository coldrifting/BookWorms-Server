namespace WebApplication1.Models;

public class TokenHeaderDTO()
{
    public string Alg => "HS256";
    public string Typ => "JWT";
}

public class TokenPayloadDTO(string issuer, string username, long tokenExpireTime, ICollection<string> roles)
{
    public string Iss => issuer;
    public string Sub => username;
    public long Exp => tokenExpireTime;
    public ICollection<string> Roles => roles;
}