using System;

namespace Terraria.WorldFile.V319.Model;

public sealed record LegacySign(int X, int Y, string Text)
{
  public string Text { get; init; } = Text ?? throw new ArgumentNullException(nameof(Text));
}
