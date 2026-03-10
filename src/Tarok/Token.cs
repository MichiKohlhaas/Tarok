namespace Tarok;

public sealed class Token
{
    public TokenEnum Type { get; private set; }
    public string Lexeme { get; private set; }
    public object? Literal { get; private set; }
    public bool Reversed { get; set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    
    public Token(TokenEnum type, string lexeme, object? literal, int row, int column)
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Row = row;
        Column = column;
    }
}