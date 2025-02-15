namespace BookwormsServer.Models.Data;

public record TokenHeaderResponse(string Alg = "HS256", string Typ = "JWT");
public record TokenPayloadResponse(string Iss, string Sub, long Exp, string Role);