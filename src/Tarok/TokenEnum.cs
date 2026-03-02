namespace Tarok;

public enum TokenEnum
{
    MajorArcana,
    MinorArcana,
    Rank,
    // Suit
    SwordCard,
    WandCard,
    CupCard,
    CoinCard,
    
    EOF,
    StartOfStatement,
    EndOfStatement,
    
    // Keywords
    Reversed,
    Read,
    Spread,
    
    // Literals
    Identifier,
    String,
    Boolean,
    Number,
    Trig,
    Array,
}