using Tarok.Enums;
using Tarok.Errors;

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
internal class Lexer
{
    private static readonly Dictionary<string, Suit> SuitKeywords = new()
    {
        { "P", Suit.Pentacles },
        { "C", Suit.Cups },
        { "S", Suit.Swords },
        { "W", Suit.Wands },
    };

    private static readonly Dictionary<string, Trump> RomanNumeralKeywords = new()
    {
        { "0", Trump.Fool },
        { "I", Trump.Magician },
        { "II", Trump.HighPriestess },
        { "III", Trump.Empress },
        { "IV", Trump.Emperor },
        { "V", Trump.Hierophant },
        { "VI", Trump.Lovers },
        { "VII", Trump.Chariot },
        { "VIII", Trump.Strength },
        { "IX", Trump.Hermit },
        { "x", Trump.WheelOfFortune },
        { "XI", Trump.Justice },
        { "XII", Trump.HangedMan },
        { "XIII", Trump.Death },
        { "XIV", Trump.Temperance },
        { "XV", Trump.Devil },
        { "XVI", Trump.Tower },
        { "XVII", Trump.Star },
        { "XVIII", Trump.Moon },
        { "XIX", Trump.Sun },
        { "XX", Trump.Judgement },
        { "XXI", Trump.World },
    };
    
    private int _start = 0; // first character being scanned
    private int _current = 0; // character currently considered
    private int _row = 1;
    private int _column = 0;
    private readonly List<Token> _tokens = [];

    // Rank numeral buffer
    private readonly List<char> _rnBuffer = []; 
    
    internal readonly List<TokenError> Errors = [];
    
    internal string Source { get; set; }

    private bool IsAtEnd => _current >= Source.Length;

    /// <summary>
    /// Cells[height, width] = row, columns
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    internal List<Token> ScanGrid(string[,] cells)
    {
        _tokens.Clear();
        for (var row = 0; row < cells.GetLength(0); row++)
        {
            for (var col = 0; col < cells.GetLength(1); col++)
            {
                _row = row; 
                _column = col;
                _start = _current;
                
                Source = cells[row, col];
                ScanToken();
                _current = 0;
                _rnBuffer.Clear();
            }
        }
    
        _tokens.Add(new Token(TokenEnum.EOF, null!, cells.GetLength(0), cells.GetLength(1)));
        return _tokens;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }
        _tokens.Add(new Token(TokenEnum.EOF, null!, _row, _column));
        return _tokens;
    }
    
    /// <summary>
    /// Fix: should just validate, not call ScanToken(), which adds to _tokens.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool IsValidToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        
        var startingErrorCount = Errors.Count;
        token = token.TrimStart().TrimEnd();
        Source = token;
        _start = _current;
        
        while (!IsAtEnd)
        {
            ScanToken();
        }

        _current = 0;
        _start = 0;
        _rnBuffer.Clear();
        return Errors.Count == startingErrorCount;
    }

    private void ScanToken()
    {
        if (string.IsNullOrEmpty(Source))
        {
            _tokens.Add(new Token(TokenEnum.Empty, null, _row, _column));
            return;
        }
        var c = Advance();
        var isReversed = false;
        if (c.Equals('@'))
        {
            isReversed = true;
            _start++;
            c = Advance();
        }
        
        switch (c)
        {
            case 'I':
            case 'V':
            case 'X':
                ScanMajorArcana(isReversed);
                break;
            default:
                if (IsDigit(c))
                {
                    if (c.Equals('0')) ScanMajorArcana(isReversed);
                    else ScanMinorArcana(isReversed);
                }
                else Errors.Add(new TokenError("Unexpected character: " + c, _row, _column));
                break;
        }
    }

    private void ScanMinorArcana(bool isReversed)
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
        var containsKey = SuitKeywords.TryGetValue(suitChar.ToUpper(), out var type);
        if (!containsKey)
        {
            Errors.Add(new TokenError($"Unknown Suit: {suitChar}. Suits are 'W', 'S', 'P', 'C'", _row, _column));
            return;
        }
        
        var minor = new MinorArcana(rankType, type, isReversed);
        
        AddToken(TokenEnum.MinorArcana, minor);
    }

    private void ScanMajorArcana(bool isReversed)
    {
        while(IsAlpha(Peek()) && !IsAtEnd) Advance();
        
        var parsedString = Source[_start.._current];
        var containsKey = RomanNumeralKeywords.TryGetValue(parsedString.ToUpper(), out var type);

        if (!containsKey)
        {
            Errors.Add(new TokenError($"Unknown Card: {parsedString}.", _row, _column));
            return;
        }
        
        var major = new MajorArcana(type, isReversed);
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

    private void AddToken(TokenEnum token, Arcana? arcana = null)
    {
        //var text = Source[_start.._current];
        var validToken = new Token(token, arcana!, _row, _column);
        _tokens.Add(validToken);
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

