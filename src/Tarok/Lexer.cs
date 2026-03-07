namespace Tarok;

public class Lexer
{
    private static Dictionary<string, TokenEnum> keywords = new()
        {
            { "Fool", TokenEnum.MajorArcana},
            { "Magician", TokenEnum.MajorArcana},
            { "Priestess", TokenEnum.MajorArcana},
            { "Empress", TokenEnum.MajorArcana},
            { "Emperor", TokenEnum.MajorArcana},
            { "Hierophant", TokenEnum.MajorArcana},
            { "Lovers", TokenEnum.MajorArcana},
            { "Chariot", TokenEnum.MajorArcana},
            { "Strength", TokenEnum.MajorArcana},
            { "Hermit", TokenEnum.MajorArcana},
            { "Fortune", TokenEnum.MajorArcana},
            { "Justice", TokenEnum.MajorArcana},
            { "Hanged Man", TokenEnum.MajorArcana},
            { "Death", TokenEnum.MajorArcana},
            { "Temperance", TokenEnum.MajorArcana},
            { "Devil", TokenEnum.MajorArcana},
            { "Tower", TokenEnum.MajorArcana},
            { "Star", TokenEnum.MajorArcana},
            { "Moon", TokenEnum.MajorArcana},
            { "Sun", TokenEnum.MajorArcana},
            { "Judgment", TokenEnum.MajorArcana},
            { "World", TokenEnum.MajorArcana},
            
            { "Coins", TokenEnum.CoinCard },
            { "Cups", TokenEnum.CupCard },
            { "Swords", TokenEnum.SwordCard },
            { "Wands", TokenEnum.WandCard },
            
            { "reversed", TokenEnum.Reversed },
            { "spread", TokenEnum.Spread },
            { "read",  TokenEnum.Read },
            
            { "PI", TokenEnum.Number },
            { "PHI", TokenEnum.Number },
            { "SIN", TokenEnum.Trig },
            { "TAN", TokenEnum.Trig },
            { "COS", TokenEnum.Trig },
            { "ARCSIN", TokenEnum.Trig },
            { "ARCCOS", TokenEnum.Trig },
            { "ARCTAN", TokenEnum.Trig },
        };
    
    private int _start = 0; // first character being scanned
    private int _current = 0; // character currently considered
    private int _line = 1;
    private int _column = 0;
    private readonly List<Token> _tokens = [];

    // Roman numeral buffer
    private List<char> _rnBuffer = []; 
    
    public readonly List<TokenError> Errors = [];
    
    public string Source { get; set; }

    private bool IsAtEnd => _current >= Source.Length;

    public async Task LoadScript(string filePath)
    {
        try
        {
            using StreamReader reader = new(filePath);
            var text = await reader.ReadToEndAsync();
            Source = text;
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
            Errors.Add(new TokenError(e.Message, _line, _column));
        }
    }
    
    

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenEnum.EOF, "", null, _line, _column));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case 'I':
            case 'V':
            case 'X':
                MinorArcana();
                break;
            case '"':
                StringLiteral();
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            default:
                if (IsDigit(c))
                {
                    NumeralLiteral();
                }
                else if (IsAlpha(c))
                {
                    Identify();
                }
                Errors.Add(new TokenError("Unexpected character: " + c, _line, _column));
                break;
        }
    }

    private void StringLiteral()
    {
        while (!Peek().Equals('"') && !IsAtEnd)
        {
            if (Peek().Equals('\n'))
            {
                _column = 1;
                _line++;
            }
            Advance();
        }
        if (IsAtEnd)
        {
            Errors.Add(new TokenError("Unexpected end of string", _line, _column));
        }
        
        // the closing '"'
        Advance();
        
        // Trim the quotes to get our string
        var value = Source[(_start + 1)..(_current - 1)];
        AddToken(TokenEnum.String, value);
    }

    private void NumeralLiteral()
    {
        while (IsDigit(Peek())) Advance();
        
        // fractional/decimal
        if (Peek().Equals('.') && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }
        AddToken(TokenEnum.Number, double.Parse(Source[(_start)..(_current)]));
    }

    private void Identify()
    {
        while(IsAlpha(Peek())) Advance();
        // Hanged Man
        if (PeekNext().Equals('M'))
        {
            
        }
        // Major Arcana
        
        // Keyword 
        
        // other
        AddToken(TokenEnum.Identifier);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd) return false;
        if (Source[_current] != expected) return false;
        
        _current++;
        return true;
    }

    private void MinorArcana()
    {
        _rnBuffer.Add(Source[_current - 1]);
        
        // Scan for Rank
        while (!Peek().Equals(' ') && !IsAtEnd)
        {
            _rnBuffer.Add(Advance());
        }

        if (IsAtEnd)
        {
            Errors.Add(new TokenError("Unexpected end of statement", _line, _column));
        }
        
        // skip the ' ' b/w the rank and suit
        if (Peek().Equals(' ')) Advance();
        else
        {
            //_column++;
            Errors.Add(new TokenError("Unexpected end of string", _line, _column));
            return;
        }
        var rank = string.Join("", _rnBuffer);
        var rankType = Enum.Parse<TokenEnum>(rank);
        
        var suitIndex = _current;
        
        // Scan for Suit
        while (IsAlpha(Peek())) Advance();
        
        var text = Source[suitIndex.._current];
        var containsKey = keywords.TryGetValue(text, out var type);
        if (!containsKey)
        {
            Errors.Add(new TokenError($"Unknown keyword: {text}", _line, _column));
            return;
        }
        //_tokens.Add(new Token(type, text, null, _line, _column));
        var minor = new MinorArcana(rankType, type);
        AddToken(TokenEnum.MinorArcana, minor);
    }

    private bool IsAlpha(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private char Advance()
    {
        _column++;
        return Source[_current++];
    }

    private void AddToken(TokenEnum token, object? literal = null)
    {
        var text = Source[_start.._current];
        _tokens.Add(new Token(token, text, literal, _line, _column));
    }

    private char Peek()
    {
        return IsAtEnd ? '\0' : Source[_current];
    }

    private char PeekNext()
    {
        return _current + 1 >= Source.Length ? '\0' : Source[_current + 1];
    }
}

