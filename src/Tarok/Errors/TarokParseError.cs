namespace Tarok.Errors;

public class TarokParseError(string message, Token token) : TarokError(message)
{
    public string PrintToken()
    {
        return $"Token type: {token.Arcana}.\nArcana: {token.Arcana}";
    } 
}