using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Definitions;

public sealed class WorldSecretSeedRegistryDefinitionsProjection
{
  private static readonly IReadOnlyList<WorldSecretSeedDefinition> DefaultDefinitions =
    Array.AsReadOnly(
    [
      Create("paint-everything-gray", "paintEverythingGray", "2htOIVagY/7JFx7acMpyUR6D3qJDr/u+", 342),
      Create("paint-everything-negative", "paintEverythingNegative", "YJayFFSdWEl66+rlFoWJRNvBHJi8gHnx", 344),
      Create("coat-everything-echo", "coatEverythingEcho", "5Czr2vSNyB9hJd1yob+TYo0qqH/5U2P9", 346),
      Create("coat-everything-illuminant", "coatEverythingIlluminant", "5YXhKErRZovhjJkrP9fptrVHbNc1oSSn", 348),
      Create("no-surface", "noSurface", "cptECrPRxYeNTULJULs4gVoKdRsf3c3n", 350),
      Create("extra-living-trees", "extraLivingTrees", "QQN1FbxlHeUCXPZc51GYvn8G5GXOJcny", 352),
      Create("extra-floating-islands", "extraFloatingIslands", "0ebq4RCzI3PVaUPOT0f6/+vkXEaoLz2U", 354),
      Create("error-world", "errorWorld", "GkviuS3QN0pyESRJdjIs6oC8s8hOhUXw", 356),
      Create("graveyard-bloodmoon-start", "graveyardBloodmoonStart", "N8G20sWOkIa7ZP0rS/jopLpe9180N6Tx", 358),
      Create("surface-is-in-space", "surfaceIsInSpace", "io2s6kMi4L7ZCDYZGP1Hc8nEWuYW4gp5", 360),
      Create("rains-for-a-year", "rainsForAYear", "xYBNU5Soje9VhQHNQXETDKbwlc+7XZau", 362),
      Create("bigger-abandoned-houses", "biggerAbandonedHouses", "vWb/t7nNF+tnjgr5VgY2hi0HcT1j3kvC", 364),
      Create("random-spawn", "randomSpawn", "zSwnCH9E121+S6VQdB0k20E7IPdtobls", 366),
      Create("add-teleporters", "addTeleporters", "+URq9gxzcyHxAXVqdwl1fz8wgPYYu0Wx", 368),
      Create("start-in-hardmode", "startInHardmode", "6kX2PJe0FWt3i0fp0tVBh5jt84ozLXBo", 370),
      Create("no-infection", "noInfection", "m1gQVuUnIRW083pnfFdnN3DPsg1qFYHZ", 372),
      Create("hallow-on-the-surface", "hallowOnTheSurface", "KYvKIk2LK0oyNY86m+uPhKQ7QbzFmDsR", 374),
      Create("world-is-infected", "worldIsInfected", "kbxnychxHNDcoyFHhxM9OJHRxis6mFF/", 376),
      Create("surface-is-mushrooms", "surfaceIsMushrooms", "e48+tRi5DqzRkBPk3yq9udBG/kaYOQaB", 378),
      Create("surface-is-desert", "surfaceIsDesert", "eyGmBQhQ9QnE7UsIib1QmnNRVBNmQtMi", 380),
      Create("poo-everywhere", "pooEverywhere", "Iubz1XcBvsfPjSZucIJ3hCDFFEpjG57w", 382),
      Create("no-spider-caves", "noSpiderCaves", "SPlOdka0fv8wUovao6u3VB7ZS+IbcPDu", 384),
      Create("actually-no-traps", "actuallyNoTraps", "AoEz0g1XX0V/nJwcaN2RWwUf/6ghr9pT", 386),
      Create("rainbow-stuff", "rainbowStuff", "6lK0Tn4t2UlklesGiJ94617yKvk01ICB", 388),
      Create("dig-extra-holes", "digExtraHoles", "MucLvCERZix3rfcwUH68HDtuFYukiTv9", 390),
      Create("round-landmasses", "roundLandmasses", "VSN8nV180t6PgabWDl4Uf55I1vu97JRD", 392),
      Create("extra-liquid", "extraLiquid", "ZYO3rUjSeCaaBrCE8Bv0FBtkjigLMz90", 394),
      Create("portal-gun-in-chests", "portalGunInChests", "ALdQZ+bxQA4VdfjVfdhO/sm9q3sZD9dJ", 396),
      Create("world-is-frozen", "worldIsFrozen", "eH2IYQwQyOud0hyoTPaeVsqYlAP7MvbS", 398),
      Create("halloween-gen", "halloweenGen", "Z4Odmvd5lScy/KGXHUO2nvqA9l3KRvm8", 400),
      Create("endless-halloween", "endlessHalloween", "KNSxbK83ZXH41aUhWLti9OFMxoMrCV1s", 402),
      Create("endless-christmas", "endlessChristmas", "gkN386qfe3u1qqQDpGsUu3DsRkEBpD1R", 404),
      Create("vampirism", "vampirism", "4eijvDtfcSl66CDifYSVP3WBZm9OLBoW", 406),
      Create("team-based-spawns", "teamBasedSpawns", "HnTdmrZ5OT1ldA3r0w3dCgrdLnJBtBSD", 408),
      Create("dual-dungeons", "dualDungeons", "ypBuvKpqKay//OvhG2COriSpGT7f4YY3", 410)
    ]);

  public WorldSecretSeedRegistryDefinitionsProjection(
    IReadOnlyList<WorldSecretSeedDefinition>? definitions = null)
  {
    IReadOnlyList<WorldSecretSeedDefinition> source = definitions ?? DefaultDefinitions;
    ArgumentNullException.ThrowIfNull(source);
    Definitions = Array.AsReadOnly(
      new List<WorldSecretSeedDefinition>(source).ToArray());
  }

  public static WorldSecretSeedRegistryDefinitionsProjection Version4 { get; } = new();

  public IReadOnlyList<WorldSecretSeedDefinition> Definitions { get; }

  public bool TryGet(
    string variant,
    out WorldSecretSeedDefinition definition)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    foreach (WorldSecretSeedDefinition candidate in Definitions)
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

  private static WorldSecretSeedDefinition Create(
    string variant,
    string localizationSuffix,
    string opaqueCode,
    int sourceLine)
  {
    return new WorldSecretSeedDefinition(
      variant,
      $"SecretSeedDescription.{localizationSuffix}",
      opaqueCode,
      $"WorldGen.cs:{sourceLine}");
  }
}
