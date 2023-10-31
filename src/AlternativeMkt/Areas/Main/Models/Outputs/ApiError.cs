namespace AlternativeMkt.Main.Models;

public class ApiError
{
    public ApiError(string message) {
        Message = message;
    }
    public string Message { get; private set; }
}