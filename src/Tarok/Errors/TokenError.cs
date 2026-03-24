namespace Tarok.Errors;

public class TokenError(string message, int line, int column) : TarokError(message); 