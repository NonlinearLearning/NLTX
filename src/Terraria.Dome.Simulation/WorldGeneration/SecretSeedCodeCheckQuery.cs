using System;
using System.Text.RegularExpressions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static partial class SecretSeedCodeCheckQuery
{
  public static bool Matches(string? input, string expectedCode)
  {
    ArgumentNullException.ThrowIfNull(expectedCode);
    if (string.IsNullOrWhiteSpace(input))
    {
      return false;
    }

    string normalized = AlphaNumericRegex().Replace(input.ToLowerInvariant(), string.Empty);
    return normalized.Length > 0 &&
      StringComparer.Ordinal.Equals(normalized, expectedCode);
  }

  [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
  private static partial Regex AlphaNumericRegex();
}
