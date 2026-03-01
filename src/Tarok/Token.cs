namespace Tarok;

public sealed class Token
{
    public TokenEnum Type { get; private set; }
    public string Lexeme { get; private set; }
    public object Literal { get; private set; }
    public int Line { get; private set; }
    public int Column { get; private set; }
    
    public Token(TokenEnum type, string lexeme, object literal, int line, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
        Column = column;
    }
}