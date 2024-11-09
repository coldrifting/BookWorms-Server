namespace WebApplication1.Models;

public class ErrorDTO(string error, string description)
{
    public string Error { get; set; } = error;
    public string Description { get; set; } = description;

    public string Json()
    {
		return "{" +
          "\"error\": \"" + Error + "\"," +
          "\"description\": \"" + Description + "\"" +
          "}";
    }
}