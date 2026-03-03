namespace Tarok;

public enum TokenEnum
{
    MajorArcana,
    MinorArcana,
    
    // Suit
    SwordCard,
    WandCard,
    CupCard,
    CoinCard,
    
    // Ranks
    I,
    II,
    III,
    IV,
    V,
    VI,
    VII,
    VIII,
    IX,
    X,
    
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