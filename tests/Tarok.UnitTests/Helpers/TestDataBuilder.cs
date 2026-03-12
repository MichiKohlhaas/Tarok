namespace Tarok.UnitTests.Helpers;

public class TestDataBuilder
{
    private static readonly Random Random = new();
    
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

    private static string CreateRandomMajorArcana()
    {
        var number = Random.Next(0, 22);
        return number == 0 ? "0" : ToRomanNumeral(number);
    }

    private static string CreateRandomMinorArcana()
    {
        var number = Random.Next(1, 15);
        var suit =  Random.Next(1, 5);
        return number.ToString() + ToSuit(suit);
    }
    
    private static char ToSuit(int number)
    {
        var suits = new (int Value, char Suit)[]
        {
            (1, 'C'),
            (2, 'P'),
            (3, 'S'),
            (4, 'W'),
        };
        
        var result = '\0';
        foreach (var (value, numeral) in suits)
        {
            while (number >= value)
            {
                result += numeral;
                number -= value;
            }
        }
        return result;
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