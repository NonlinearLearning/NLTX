using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record WorldGenerationRequest
{
  public WorldGenerationRequest(
    WorldMetadata metadata,
    int spawnX,
    int surfaceY,
    string seedVariant,
    int randomStreamVersion,
    long? generationId,
    WorldRuleSnapshotComponent? rules,
    int? rockLayerY,
    IReadOnlyList<int>? dirtWallSurfaceOffsetChanges,
    LegacyTerrainRuntimeProfile? terrainProfile,
    BiomeSurfaceDefinition? biomeSurfaceDefinition,
    WorldGenerationDistanceDefaults? distanceDefaults,
    int worldGenParamEvil)
    : this(
      metadata,
      spawnX,
      surfaceY,
      seedVariant,
      randomStreamVersion,
      generationId,
      rules,
      rockLayerY,
      dirtWallSurfaceOffsetChanges,
      terrainProfile,
      biomeSurfaceDefinition,
      distanceDefaults,
      worldGenParamEvil,
      isDontStarveWorld: false,
      isSkyblockWorld: false,
      isIceBiomeWorld: false,
      isNoSurfaceWorld: false,
      isSurfaceDesertWorld: false,
      isTenthAnniversaryWorld: false)
  {
  }

  public WorldGenerationRequest(
    WorldMetadata metadata,
    int spawnX,
    int surfaceY,
    string seedVariant = "default",
    int randomStreamVersion = 1,
    long? generationId = null,
    WorldRuleSnapshotComponent? rules = null,
    int? rockLayerY = null,
    IReadOnlyList<int>? dirtWallSurfaceOffsetChanges = null,
    LegacyTerrainRuntimeProfile? terrainProfile = null,
    BiomeSurfaceDefinition? biomeSurfaceDefinition = null,
    WorldGenerationDistanceDefaults? distanceDefaults = null,
    int worldGenParamEvil = -1,
    bool isDontStarveWorld = false,
    bool isSkyblockWorld = false,
    bool isIceBiomeWorld = false,
    bool isNoSurfaceWorld = false,
    bool isSurfaceDesertWorld = false,
    bool isTenthAnniversaryWorld = false)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    if (randomStreamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(randomStreamVersion));
    }

    if (worldGenParamEvil is < -1 or > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(worldGenParamEvil));
    }

    if (spawnX < 0 || spawnX >= metadata.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnX));
    }

    if (surfaceY < 0 || surfaceY >= metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceY));
    }

    int defaultRockLayerY = surfaceY + Math.Max(1, (metadata.Height - surfaceY) / 3);
    int selectedRockLayerY = rockLayerY ?? defaultRockLayerY;
    if (selectedRockLayerY <= surfaceY || selectedRockLayerY >= metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayerY));
    }

    IReadOnlyList<int>? frozenDirtWallSurfaceOffsetChanges = null;
    if (dirtWallSurfaceOffsetChanges is not null)
    {
      if (dirtWallSurfaceOffsetChanges.Count != metadata.Width - 2)
      {
        throw new ArgumentException(
          "A dirt wall offset change is required for every non-edge column.",
          nameof(dirtWallSurfaceOffsetChanges));
      }

      int[] copiedChanges = new int[dirtWallSurfaceOffsetChanges.Count];
      for (int index = 0; index < copiedChanges.Length; index++)
      {
        int offsetChange = dirtWallSurfaceOffsetChanges[index];
        if (offsetChange < -1 || offsetChange > 1)
        {
          throw new ArgumentOutOfRangeException(nameof(dirtWallSurfaceOffsetChanges));
        }

        copiedChanges[index] = offsetChange;
      }

      frozenDirtWallSurfaceOffsetChanges = Array.AsReadOnly(copiedChanges);
    }

    terrainProfile?.Validate(metadata);

    WorldRuleSnapshotComponent selectedRules = rules ??
      new WorldRuleSnapshotComponent(0, seedVariant, false);
    if (!StringComparer.Ordinal.Equals(selectedRules.SecretSeedVariant, seedVariant))
    {
      throw new ArgumentException(
        "World rules must use the same secret-seed variant as the generation request.",
        nameof(rules));
    }

    if (!StringComparer.Ordinal.Equals(metadata.SeedVariant, seedVariant))
    {
      throw new ArgumentException(
        "World metadata must use the same seed variant as the generation request.",
        nameof(seedVariant));
    }

    Metadata = metadata.RockLayer == selectedRockLayerY
      ? metadata
      : metadata.WithRockLayer(selectedRockLayerY);
    SpawnX = spawnX;
    SurfaceY = surfaceY;
    RockLayerY = selectedRockLayerY;
    SeedVariant = seedVariant;
    RandomStreamVersion = randomStreamVersion;
    GenerationId = generationId ?? metadata.WorldId;
    Rules = selectedRules;
    DirtWallSurfaceOffsetChanges = frozenDirtWallSurfaceOffsetChanges;
    TerrainProfile = terrainProfile;
    BiomeSurfaceDefinition = biomeSurfaceDefinition;
    DistanceDefaults = distanceDefaults ?? WorldGenerationDistanceDefaults.Version4;
    WorldGenParamEvil = worldGenParamEvil;
    IsDontStarveWorld = isDontStarveWorld;
    IsSkyblockWorld = isSkyblockWorld || Metadata.IsSkyblockWorld == true;
    IsIceBiomeWorld = isIceBiomeWorld;
    IsNoSurfaceWorld = isNoSurfaceWorld;
    IsSurfaceDesertWorld = isSurfaceDesertWorld;
    IsTenthAnniversaryWorld = isTenthAnniversaryWorld;
    if (GenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }
  }

  public WorldMetadata Metadata { get; }
  public int SpawnX { get; }
  public int SurfaceY { get; }
  public int RockLayerY { get; }
  public string SeedVariant { get; }
  public int RandomStreamVersion { get; }
  public long GenerationId { get; }
  public WorldRuleSnapshotComponent Rules { get; }
  public IReadOnlyList<int>? DirtWallSurfaceOffsetChanges { get; }
  public LegacyTerrainRuntimeProfile? TerrainProfile { get; }
  public BiomeSurfaceDefinition? BiomeSurfaceDefinition { get; }
  public WorldGenerationDistanceDefaults DistanceDefaults { get; }
  public int WorldGenParamEvil { get; }
  public bool IsDontStarveWorld { get; }
  public bool IsSkyblockWorld { get; }
  public bool IsGoodWorld => Metadata.IsGoodWorld == true;
  public bool IsIceBiomeWorld { get; }
  public bool IsNoSurfaceWorld { get; }
  public bool IsSurfaceDesertWorld { get; }
  public bool IsTenthAnniversaryWorld { get; }
}
