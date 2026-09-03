using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyStageFingerprintComparisonResult(
  int ComparedStageCount,
  int MissingOracleStageCount,
  int MismatchCount,
  bool Compared,
  bool Matches);

public static class LegacyStageFingerprintComparison
{
  public static LegacyStageFingerprintComparisonResult Compare(
    IReadOnlyDictionary<string, string> generated,
    IReadOnlyDictionary<string, string>? oracle)
  {
    ArgumentNullException.ThrowIfNull(generated);
    if (oracle is null)
    {
      return new LegacyStageFingerprintComparisonResult(
        0,
        generated.Count,
        0,
        false,
        false);
    }

    int compared = 0;
    int missing = 0;
    int mismatches = 0;
    foreach (KeyValuePair<string, string> stage in generated)
    {
      if (!oracle.TryGetValue(stage.Key, out string? oracleFingerprint))
      {
        missing++;
        continue;
      }

      compared++;
      if (!StringComparer.OrdinalIgnoreCase.Equals(stage.Value, oracleFingerprint))
      {
        mismatches++;
      }
    }

    return new LegacyStageFingerprintComparisonResult(
      compared,
      missing,
      mismatches,
      compared > 0,
      compared > 0 && missing == 0 && mismatches == 0);
  }
}
