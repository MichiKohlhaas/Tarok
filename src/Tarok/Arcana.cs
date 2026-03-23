using Tarok.Enums;
namespace Tarok;

public abstract record Arcana(bool IsReversed)
{
    public abstract bool Equals(Arcana? other);

    public override int GetHashCode()
    {
        return IsReversed.GetHashCode();
    }
}

public sealed record MajorArcana(Trump Card, bool IsReversed) : Arcana(IsReversed)
{
    public bool Equals(MajorArcana? other)
    {
        if (other is null) return false;
        return ReferenceEquals(this, other) || (Card.Equals(other.Card) && IsReversed == other.IsReversed);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), (int)Card);
    }

    public override string ToString()
    {
        return $"\t-MajorArcana\n\t-Trump: {Card}\n\t-Reversed: {IsReversed}";
    }
}

public sealed record MinorArcana(int Rank, Suit Suit, bool IsReversed) : Arcana(IsReversed)
{
    public bool Equals(MinorArcana? other)
    {
        if  (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Rank == other.Rank && Suit == other.Suit) return IsReversed == other.IsReversed;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Rank, (int)Suit);
    }

    public override string ToString()
    {
        return $"\t-MinorArcana\n\t-Rank: {Rank}\n\t-Suit: {Suit}\n\t-Reversed: {IsReversed}";
    }
}
