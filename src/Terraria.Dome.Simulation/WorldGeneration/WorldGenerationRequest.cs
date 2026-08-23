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
    string seedVariant = "default",
    int randomStreamVersion = 1,
    long? generationId = null,
    WorldRuleSnapshotComponent? rules = null,
    int? rockLayerY = null,
    IReadOnlyList<int>? dirtWallSurfaceOffsetChanges = null,
    LegacyTerrainRuntimeProfile? terrainProfile = null)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    if (randomStreamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(randomStreamVersion));
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

    Metadata = metadata;
    SpawnX = spawnX;
    SurfaceY = surfaceY;
    RockLayerY = selectedRockLayerY;
    SeedVariant = seedVariant;
    RandomStreamVersion = randomStreamVersion;
    GenerationId = generationId ?? metadata.WorldId;
    Rules = rules ?? new WorldRuleSnapshotComponent(0, seedVariant, false);
    DirtWallSurfaceOffsetChanges = frozenDirtWallSurfaceOffsetChanges;
    TerrainProfile = terrainProfile;
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
}
