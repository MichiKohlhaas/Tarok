namespace Tarok.Errors;

public class TarokParseError(string message, Token? token) : TarokError(message)
{
    public string PrintToken()
    {
        return token is null ? "No token found" : $"Token type: {token.Arcana}.\nArcana: {token.Arcana}";
    } 
}