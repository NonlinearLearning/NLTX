using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SecretSeedDefinitionRegistry
{
  private const string SourcePrefix = "WorldGen.cs:";
  private static readonly IReadOnlyList<SecretSeedDefinition> Definitions = Array.AsReadOnly(
  [
    Create("paint-everything-gray", 342), Create("paint-everything-negative", 344),
    Create("coat-everything-echo", 346), Create("coat-everything-illuminant", 348),
    Create("no-surface", 350), Create("extra-living-trees", 352),
    Create("extra-floating-islands", 354), Create("error-world", 356),
    Create("graveyard-bloodmoon-start", 358), Create("surface-is-in-space", 360),
    Create("rains-for-a-year", 362), Create("bigger-abandoned-houses", 364),
    Create("random-spawn", 366), Create("add-teleporters", 368),
    Create("start-in-hardmode", 370), Create("no-infection", 372),
    Create("hallow-on-the-surface", 374), Create("world-is-infected", 376),
    Create("surface-is-mushrooms", 378), Create("surface-is-desert", 380),
    Create("poo-everywhere", 382), Create("no-spider-caves", 384),
    Create("actually-no-traps", 386), Create("rainbow-stuff", 388),
    Create("dig-extra-holes", 390), Create("round-landmasses", 392),
    Create("extra-liquid", 394), Create("portal-gun-in-chests", 396),
    Create("world-is-frozen", 398), Create("halloween-gen", 400),
    Create("endless-halloween", 402), Create("endless-christmas", 404),
    Create("vampirism", 406), Create("team-based-spawns", 408), Create("dual-dungeons", 410)
  ]);

  public static IReadOnlyList<SecretSeedDefinition> RegisterDefaults()
  {
    return Definitions;
  }

  public static bool TryGet(string variant, out SecretSeedDefinition definition)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    foreach (SecretSeedDefinition candidate in Definitions)
    {
      if (StringComparer.Ordinal.Equals(candidate.Variant, variant))
      {
        definition = candidate;
        return true;
      }
    }

    definition = default;
    return false;
  }

  private static SecretSeedDefinition Create(string variant, int sourceLine)
  {
    return new SecretSeedDefinition(
      variant,
      $"{SourcePrefix}{sourceLine}",
      SecretSeedBehaviorStatus.UnsupportedWithEvidence);
  }
}
