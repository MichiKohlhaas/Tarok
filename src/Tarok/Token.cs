using Tarok.Enums;

namespace Tarok;

public sealed record Token(TokenEnum Type, Arcana? Arcana, int Row, int Column) : IEquatable<Token>
{
    public bool Equals(Token? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Type != other.Type) return false;
        return Arcana == other.Arcana;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Arcana, Row, Column);
    }
}