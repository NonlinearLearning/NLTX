using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldSpawnAndLandmassComponent
{
  private WorldGenerationLandmassValue[] _landmassData;
  private IReadOnlyList<WorldGenerationLandmassValue> _landmassDataView;

  public WorldSpawnAndLandmassComponent(
    long generationId,
    bool worldSpawnHasBeenRandomized = false,
    IReadOnlyList<WorldGenerationLandmassValue>? landmassData = null,
    int remixSurfaceLayerLow = 0,
    int remixSurfaceLayerHigh = 0,
    int remixMushroomLayerLow = 0,
    int remixMushroomLayerHigh = 0,
    int boulderPetsPlaced = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    WorldSpawnHasBeenRandomized = worldSpawnHasBeenRandomized;
    RemixSurfaceLayerLow = remixSurfaceLayerLow;
    RemixSurfaceLayerHigh = remixSurfaceLayerHigh;
    RemixMushroomLayerLow = remixMushroomLayerLow;
    RemixMushroomLayerHigh = remixMushroomLayerHigh;
    BoulderPetsPlaced = boulderPetsPlaced;
    _landmassData = CopyLandmassData(landmassData);
    _landmassDataView = Array.AsReadOnly(_landmassData);
  }

  public long GenerationId { get; }

  public bool WorldSpawnHasBeenRandomized { get; private set; }

  public IReadOnlyList<WorldGenerationLandmassValue> LandmassData =>
    _landmassDataView;

  public int RemixSurfaceLayerLow { get; private set; }

  public int RemixSurfaceLayerHigh { get; private set; }

  public int RemixMushroomLayerLow { get; private set; }

  public int RemixMushroomLayerHigh { get; private set; }

  public int BoulderPetsPlaced { get; private set; }

  internal void ReplaceState(
    bool worldSpawnHasBeenRandomized,
    IReadOnlyList<WorldGenerationLandmassValue>? landmassData,
    int remixSurfaceLayerLow,
    int remixSurfaceLayerHigh,
    int remixMushroomLayerLow,
    int remixMushroomLayerHigh,
    int boulderPetsPlaced)
  {
    WorldSpawnHasBeenRandomized = worldSpawnHasBeenRandomized;
    RemixSurfaceLayerLow = remixSurfaceLayerLow;
    RemixSurfaceLayerHigh = remixSurfaceLayerHigh;
    RemixMushroomLayerLow = remixMushroomLayerLow;
    RemixMushroomLayerHigh = remixMushroomLayerHigh;
    BoulderPetsPlaced = boulderPetsPlaced;
    _landmassData = CopyLandmassData(landmassData);
    _landmassDataView = Array.AsReadOnly(_landmassData);
  }

  public WorldSpawnAndLandmassSnapshot CreateSnapshot()
  {
    return new WorldSpawnAndLandmassSnapshot(
      GenerationId,
      WorldSpawnHasBeenRandomized,
      Array.AsReadOnly(CopyLandmassData(_landmassData)),
      RemixSurfaceLayerLow,
      RemixSurfaceLayerHigh,
      RemixMushroomLayerLow,
      RemixMushroomLayerHigh,
      BoulderPetsPlaced);
  }

  private static WorldGenerationLandmassValue[] CopyLandmassData(
    IReadOnlyList<WorldGenerationLandmassValue>? source)
  {
    if (source is null || source.Count == 0)
    {
      return Array.Empty<WorldGenerationLandmassValue>();
    }

    WorldGenerationLandmassValue[] copy = new WorldGenerationLandmassValue[source.Count];
    for (int index = 0; index < source.Count; index++)
    {
      copy[index] = source[index];
    }

    return copy;
  }
}
