using System;
using System.Collections.Generic;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public sealed record LegacyNetworkText
{
  public LegacyNetworkText(
    LegacyNetworkTextMode mode,
    string text,
    IReadOnlyList<LegacyNetworkText>? substitutions = null)
  {
    if (mode < LegacyNetworkTextMode.Literal ||
        mode > LegacyNetworkTextMode.LocalizationKey)
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    ArgumentNullException.ThrowIfNull(text);
    IReadOnlyList<LegacyNetworkText> normalizedSubstitutions =
      substitutions ?? Array.Empty<LegacyNetworkText>();
    if (mode == LegacyNetworkTextMode.Literal && normalizedSubstitutions.Count != 0)
    {
      throw new ArgumentException(
        "Literal NetworkText cannot contain substitutions.",
        nameof(substitutions));
    }

    Mode = mode;
    Text = text;
    Substitutions = normalizedSubstitutions;
  }

  public LegacyNetworkTextMode Mode { get; }

  public string Text { get; }

  public IReadOnlyList<LegacyNetworkText> Substitutions { get; }

  public static LegacyNetworkText Formattable(
    string text,
    params LegacyNetworkText[] substitutions)
  {
    return new LegacyNetworkText(LegacyNetworkTextMode.Formattable, text, substitutions);
  }

  public static LegacyNetworkText Literal(string text)
  {
    return new LegacyNetworkText(LegacyNetworkTextMode.Literal, text);
  }

  public static LegacyNetworkText LocalizationKey(
    string text,
    params LegacyNetworkText[] substitutions)
  {
    return new LegacyNetworkText(LegacyNetworkTextMode.LocalizationKey, text, substitutions);
  }
}
