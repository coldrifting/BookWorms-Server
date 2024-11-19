namespace BookwormsServer.Models.Data;

public class ErrorDTO(string error, string description)
{
    public string Error { get; set; } = error;
    public string Description { get; set; } = description;
}