using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWorldInfectionConversionCommand(
  long Sequence,
  int X,
  int Y,
  int ConversionType,
  bool ConvertTiles,
  bool ConvertWalls,
  string Source = "worldgen.biome.WorldIsInfected")
{
  public bool HasWork => ConvertTiles || ConvertWalls;

  public bool IsValid(WorldGridSnapshot snapshot)
  {
    return snapshot is not null &&
      snapshot.Metadata.IsInside(X, Y) &&
      Sequence >= 0 && Sequence < long.MaxValue &&
      ConversionType is 1 or 2 or 3 or 4 or 5 or 6 or 8 or 9 or 10 &&
      HasWork &&
      !string.IsNullOrWhiteSpace(Source);
  }
}
