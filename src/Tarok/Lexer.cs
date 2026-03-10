namespace Tarok;

/// <summary>
/// token      := major | minor | reversed
/// reversed   := '@' (major | minor)
/// 
/// major      := '0' | roman
/// roman      := 'I' | 'II' | 'III' | 'IV' | 'V' | 'VI' | 'VII' | 'VIII'
///             | 'IX' | 'X' | 'XI' | 'XII' | 'XIII' | 'XIV' | 'XV' | 'XVI'
///             | 'XVII' | 'XVIII' | 'XIX' | 'XX' | 'XXI'
/// 
/// minor      := rank suit
/// rank       := '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'
///             | '10' | '11' | '12' | '13' | '14'
/// suit       := 'S' | 'W' | 'P' | 'C'
/// 
/// </summary>
public class Lexer
{
    private static Dictionary<string, TokenEnum> keywords = new()
        {
            { "0", TokenEnum.MajorArcana},
            { "I", TokenEnum.MajorArcana},
            { "II", TokenEnum.MajorArcana},
            { "III", TokenEnum.MajorArcana},
            { "IV", TokenEnum.MajorArcana},
            { "V", TokenEnum.MajorArcana},
            { "VI", TokenEnum.MajorArcana},
            { "VII", TokenEnum.MajorArcana},
            { "VIII", TokenEnum.MajorArcana},
            { "IX", TokenEnum.MajorArcana},
            { "x", TokenEnum.MajorArcana},
            { "XI", TokenEnum.MajorArcana},
            { "XII", TokenEnum.MajorArcana},
            { "XIII", TokenEnum.MajorArcana},
            { "XIV", TokenEnum.MajorArcana},
            { "XV", TokenEnum.MajorArcana},
            { "XVI", TokenEnum.MajorArcana},
            { "XVII", TokenEnum.MajorArcana},
            { "XVIII", TokenEnum.MajorArcana},
            { "XIX", TokenEnum.MajorArcana},
            { "XX", TokenEnum.MajorArcana},
            { "XXI", TokenEnum.MajorArcana},
            
            { "P", TokenEnum.CoinCard },
            { "C", TokenEnum.CupCard },
            { "S", TokenEnum.SwordCard },
            { "W", TokenEnum.WandCard },
            
            { "@", TokenEnum.Reversed },
            /*{ "spread", TokenEnum.Spread },
            { "read",  TokenEnum.Read },*/
            
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
    private int _row = 1;
    private int _column = 0;
    private readonly List<Token> _tokens = [];

    // Rank numeral buffer
    private List<char> _rnBuffer = []; 
    
    public readonly List<TokenError> Errors = [];
    
    public string Source { get; set; }

    private bool IsAtEnd => _current >= Source.Length;


    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenEnum.EOF, "", null, _row, _column));
        return _tokens;
    }

    public bool IsValidToken(string token)
    {
        var isReversed = false;
        token = token.TrimStart().TrimEnd();
        
        if (string.IsNullOrEmpty(token)) return false;
        
        if (token.Contains('@')) isReversed = true;

        Source = isReversed ? token[1..] :  token;

        while (!IsAtEnd)
        {
            ScanToken();
        }
        
        return Errors.Count == 0;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case 'I':
            case 'V':
            case 'X':
                MajorArcana();
                break;
            default:
                if (IsDigit(c))
                {
                    if (c.Equals('0')) MajorArcana();
                    else MinorArcana();
                }
                else Errors.Add(new TokenError("Unexpected character: " + c, _row, _column));
                break;
        }
    }

    private void MinorArcana()
    {
        _rnBuffer.Add(Source[_current - 1]);
        
        // Scan for Rank
        while (IsDigit(Peek()) && !IsAtEnd)
        {
            _rnBuffer.Add(Advance());
        }

        if (IsAtEnd)
        {
            Errors.Add(new TokenError("Unexpected end of statement", _row, _column));
            return;
        }
        
        var rank = string.Join("", _rnBuffer);
        var rankType = int.Parse(rank);

        if (rankType is < 1 or > 14)
        {
            Errors.Add(new TokenError($"Invalid Rank numeral: {rankType}", _row, _column));
            return;
        }

        var suitIndex = _current;
        
        // Scan for Suit
        while (IsAlpha(Peek())) Advance();
        
        var suitChar = Source[suitIndex.._current];
        if (suitChar.Length > 1)
        {
            Errors.Add(new TokenError($"Suit is longer than one character: {suitChar}", _row, _column));
            return;
        }
        var containsKey = keywords.TryGetValue(suitChar.ToUpper(), out var type);
        if (!containsKey)
        {
            Errors.Add(new TokenError($"Unknown Suit: {suitChar}. Suits are 'W', 'S', 'P', 'C'", _row, _column));
            return;
        }
        
        var minor = new MinorArcana(rankType, type);
        AddToken(TokenEnum.MinorArcana, minor);
    }

    private void MajorArcana()
    {
        while(IsAlpha(Peek()) && !IsAtEnd) Advance();
        
        var parsedString = Source[_start.._current];
        var containsKey = keywords.ContainsKey(parsedString.ToUpper());

        if (!containsKey)
        {
            Errors.Add(new TokenError($"Unknown Card: {parsedString}.", _row, _column));
            return;
        }
        
        var major = new MajorArcana(parsedString);
        AddToken(TokenEnum.MajorArcana, major);
    }

    private static bool IsAlpha(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    private static bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private char Advance()
    {
        /*_column++;*/
        return Source[_current++];
    }

    private Token AddToken(TokenEnum token, object? literal = null)
    {
        var text = Source[_start.._current];
        var validToken = new Token(token, text, literal, _row, _column);
        _tokens.Add(validToken);
        return validToken;
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

/*
 * private void StringLiteral()
   {
       while (!Peek().Equals('"') && !IsAtEnd)
       {
           if (Peek().Equals('\n'))
           {
               _column = 1;
               _row++;
           }
           Advance();
       }
       if (IsAtEnd)
       {
           Errors.Add(new TokenError("Unexpected end of string", _row, _column));
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

   private bool Match(char expected)
   {
       if (IsAtEnd) return false;
       if (Source[_current] != expected) return false;
       
       _current++;
       return true;
   }
 */

