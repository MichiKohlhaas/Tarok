using Tarok.Enums;

namespace Tarok;

public sealed record Token(TokenEnum Type, Arcana? Arcana, int Row, int Column);