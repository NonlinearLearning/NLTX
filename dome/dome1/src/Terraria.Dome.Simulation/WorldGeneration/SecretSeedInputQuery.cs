using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static partial class SecretSeedInputQuery
{
  public static SecretSeedInputResult Evaluate(
    string? input,
    IReadOnlyList<(string Plaintext, string Code)> candidates)
  {
    ArgumentNullException.ThrowIfNull(candidates);
    if (string.IsNullOrWhiteSpace(input))
    {
      return new SecretSeedInputResult(false, false, string.Empty, string.Empty, -1, true);
    }

    string normalized = AlphaNumericRegex().Replace(input.ToLowerInvariant(), string.Empty);
    string displayInput = DisplayInputRegex().Replace(input, string.Empty);
    if (normalized.Length == 0)
    {
      return new SecretSeedInputResult(false, false, string.Empty, displayInput, -1, true);
    }

    for (int index = 0; index < candidates.Count; index++)
    {
      (string plaintext, string code) = candidates[index];
      if (StringComparer.Ordinal.Equals(plaintext, normalized) ||
          StringComparer.Ordinal.Equals(code, normalized))
      {
        return new SecretSeedInputResult(true, true, normalized, displayInput, index, true);
      }
    }

    return new SecretSeedInputResult(true, false, normalized, displayInput, -1, true);
  }

  [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
  private static partial Regex AlphaNumericRegex();

  [GeneratedRegex("[^a-zA-Z0-9 ]+", RegexOptions.CultureInvariant)]
  private static partial Regex DisplayInputRegex();
}
