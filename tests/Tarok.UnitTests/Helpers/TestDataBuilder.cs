using Tarok.Enums;

namespace Tarok.UnitTests.Helpers;

public class TestDataBuilder
{
    private static readonly Random Random = new();
    
    private static readonly Dictionary<char, Suit> SuitKeywords = new()
    {
        { 'P', Suit.Pentacles },
        { 'C', Suit.Cups },
        { 'S', Suit.Swords },
        { 'W', Suit.Wands },
    };
    
    public static string[,] CreateSpread()
    {
        var length = Random.Next(2, 5);
        var spread = new string[length, length];
        for (var i = 0; i < length; i++)
        {
            for (var j = 0; j < length; j++)
            {
                if (Random.Next(2) == 0) spread[i, j] = CreateRandomMajorArcana();
                else spread[i, j] = CreateRandomMinorArcana();
            }
        }
        return spread;
    }

    /* 
     * [VIII] [0] [2P] [3W] [5P] [@0] [1P] [12S]
     */
    public static string[,] CreateFoolSpread()
    {
        var Fool = "0";
        var FoolReversed = "@0";
        var TwoOfPentacles = "2P";
        var ThreeOfWands = "3W";
        var FiveOfPentacles = "5P";
        var AceOfPentacles = "1P";

        var major = "VIII";
        var minor = "12S";

        var spread = new string[1, 8];
        spread[0, 0] = major;
        spread[0, 1] = Fool;
        spread[0, 2] = TwoOfPentacles;
        spread[0, 3] = ThreeOfWands;
        spread[0, 4] = FiveOfPentacles;
        spread[0, 5] = FoolReversed;
        spread[0, 6] = AceOfPentacles;
        spread[0, 7] = minor;
        
        return spread;
    }

    /*    0    1    2     3   4     5    6    7
     * 0 [  ] [  ] [  ] [  ] [10W] [2C] [9S] [IV] 
     * 1 [XI] [2P] [3W] [5P] [I  ] [  ] [  ] [  ] 
     * 2 [  ] [  ] [  ] [  ] [13S] [4S] [8P] [XII]
     * */
    public static string[,] CreateMagicianBranchSpread()
    {
        var spread = new string[3, 8];
        spread[0, 4] = "10W";
        spread[0, 5] = "2C";
        spread[0, 6] = "9S";
        spread[0, 7] = "IV";
        spread[1, 0] = "XI";
        spread[1, 1] = "2P";
        spread[1, 2] = "3W";
        spread[1, 3] = "5P";
        spread[1, 4] = "I";
        spread[2, 4] = "13S";
        spread[2, 5] = "4S";
        spread[2, 6] = "8P";
        spread[2, 7] = "XII";
        return spread;
    }

    /*
     * 0 [XI] [2P] [3W] [5P] [I  ] [  ] [  ] [  ] 
     * 1 [  ] [  ] [  ] [  ] [13S] [4S] [8P] [XII]
     */
    public static string[,] MagicianRow0Spread()
    {
        var spread = new string[2, 8];
        spread[0, 0] = "XI";
        spread[0, 1] = "2P";
        spread[0, 2] = "3W";
        spread[0, 3] = "5P";
        spread[0, 4] = "I";
        spread[1, 4] = "13S";
        spread[1, 5] = "4S";
        spread[1, 6] = "8P";
        spread[1, 7] = "XII";
        return spread;
    }
    
    /*
     * 0    1    2     3   4     5    6    7
     * 0 [  ] [  ] [  ] [  ] [10W] [2C] [9S] [IV] 
     * 1 [XI] [2P] [3W] [5P] [I  ] [  ] [  ] [  ] 
     */
    public static string[,] MagicianRowMaxSpread()
    {
        var spread = new string[2, 8];
        spread[0, 4] = "13S";
        spread[0, 5] = "4S";
        spread[0, 6] = "8P";
        spread[0, 7] = "XII";
        spread[1, 0] = "XI";
        spread[1, 1] = "2P";
        spread[1, 2] = "3W";
        spread[1, 3] = "5P";
        spread[1, 4] = "I";
        return spread;
    }

    public static string[,] JusticRowNoMagicianSpread()
    {
        var spread = new string[2, 8];
        spread[0, 4] = "XI";
        spread[0, 5] = "4S";
        spread[0, 6] = "8P";
        spread[0, 7] = "XII";
        spread[1, 0] = "IX";
        spread[1, 1] = "2P";
        spread[1, 2] = "3W";
        spread[1, 3] = "5P";
        return spread;
    }

    /*
     * [VIII] [1P] [12S] [EOF]
     */
    public static List<Token> ExpectedTokens()
    {
        return
        [
            new Token(TokenEnum.MajorArcana, new MajorArcana(Trump.Strength, false), 0, 0),
            new Token(TokenEnum.MinorArcana, new MinorArcana(1, Suit.Pentacles, false), 0, 6),
            new Token(TokenEnum.MinorArcana, new MinorArcana(12, Suit.Swords, false), 0, 7),
            new Token(TokenEnum.EOF, null, 0, 8)
        ];
    }

    public static List<Token> CreateFoolBlock()
    {
        var tokens = new List<Token>
        {
            new(TokenEnum.MajorArcana, 
                new MajorArcana(Trump.Fool, false), 
                1, 
                1)
        };

        var number = Random.Next(3, 15);
        for (var i = 0; i < number; i++)
        {
            tokens.Add(CreateRandomMinorArcanaToken());
        }
        
        // close the marker
        tokens.Add(new Token(
            TokenEnum.MajorArcana, 
            new MajorArcana(Trump.Fool, true), 
            1, 
            1)
        );
        
        var suit = Random.Next(1, 4) switch
        {
            1 => Suit.Cups,
            2 => Suit.Wands,
            3 => Suit.Swords,
            4 => Suit.Pentacles,
        };
        
        // Ace card for memory slot
        tokens.Add(new Token(
            TokenEnum.MinorArcana,
            new MinorArcana(1, suit, false),0,0));
        
        return tokens;
    }

    private static Token CreateRandomMinorArcanaToken()
    {
        var number = Random.Next(1, 15);
        var suitNum = Random.Next(1, 5);
        var randomSuit = ToSuit(suitNum);
        var minor = new MinorArcana(number, SuitKeywords[randomSuit], false);
        
        return new Token(TokenEnum.MinorArcana, minor, 0, 0);
    }

    private static string CreateRandomMajorArcana()
    {
        var number = Random.Next(1, 22);
        return ToRomanNumeral(number);
    }

    private static string CreateRandomMinorArcana()
    {
        var number = Random.Next(1, 15);
        var suit =  Random.Next(1, 5);
        return number.ToString() + ToSuit(suit);
    }
    
    private static char ToSuit(int number)
    {
        return number switch
        {
            1 => 'C',
            2 => 'P',
            3 => 'S',
            4 => 'W',
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, null)
        };
    }

    private static string ToRomanNumeral(int number)
    {
        var romanNumerals = new (int Value, string Numeral)[]
        {
            (21, "XXI"),
            (20, "XX"),
            (19, "XIX"),
            (18, "XVIII"),
            (17, "XVII"),
            (16, "XVI"),
            (15, "XV"),
            (14, "XIV"),
            (13, "XIII"),
            (12, "XII"),
            (11, "XI"),
            (10, "X"),
            (9, "IX"),
            (8, "VIII"),
            (7, "VII"),
            (6, "VI"),
            (5, "V"),
            (4, "IV"),
            (3, "III"),
            (2, "II"),
            (1, "I")
        };
        
        var result = string.Empty;
        foreach (var (value, numeral) in romanNumerals)
        {
            while (number >= value)
            {
                result += numeral;
                number -= value;
            }
        }
        return result;
    }
}