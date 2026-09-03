using System;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldCompatibility.Model;

public sealed record CompatibilitySignSnapshot(int X, int Y, string Text)
{
  public string Text { get; init; } = Text ?? throw new ArgumentNullException(nameof(Text));

  internal static CompatibilitySignSnapshot From(LegacySign sign)
  {
    ArgumentNullException.ThrowIfNull(sign);
    return new CompatibilitySignSnapshot(sign.X, sign.Y, sign.Text);
  }
}
