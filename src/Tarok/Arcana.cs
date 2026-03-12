using Tarok.Enums;
namespace Tarok;

public abstract record Arcana(bool IsReversed);
public sealed record MajorArcana(Trump Card, bool IsReversed) : Arcana(IsReversed);
public sealed record MinorArcana(int Rank, Suit Suit, bool IsReversed) : Arcana(IsReversed);
