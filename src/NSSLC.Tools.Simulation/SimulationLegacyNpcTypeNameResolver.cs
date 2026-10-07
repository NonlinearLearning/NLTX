using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.SimulationHost;

/// <summary>Resolves legacy names supported by the finite simulation content catalog.</summary>
internal static class SimulationLegacyNpcTypeNameResolver
{
  private static readonly IReadOnlyDictionary<string, int> NetIdByLegacyTypeName =
    new Dictionary<string, int>(StringComparer.Ordinal)
    {
      ["Blue Slime"] = SimulationContentSupportManifest.BlueSlimeNetId,
      ["Demon Eye"] = SimulationContentSupportManifest.DemonEyeNetId,
      ["Green Slime"] = SimulationContentSupportManifest.GreenSlimeNetId,
      ["Guide"] = 22,
      ["Old Man"] = 37,
      ["Zombie"] = 3,
    };

  public static bool TryResolveNetId(string legacyTypeName, out int netId)
  {
    if (string.IsNullOrEmpty(legacyTypeName))
    {
      netId = default;
      return false;
    }

    return NetIdByLegacyTypeName.TryGetValue(legacyTypeName, out netId);
  }
}
