using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyWorldInfectionConversionInput(
  int WorldSurfaceY,
  int RockLayerY,
  int UnderworldLayerY,
  bool NoInfection,
  bool NoSurface,
  bool HallowOnSurface,
  bool DrunkWorld,
  bool Crimson,
  bool CrimsonLeft,
  bool SkyblockWorld)
{
  public void Validate(WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (WorldSurfaceY < 0 || WorldSurfaceY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(WorldSurfaceY));
    }

    if (RockLayerY <= WorldSurfaceY || RockLayerY >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(RockLayerY));
    }

    if (UnderworldLayerY <= RockLayerY || UnderworldLayerY > snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(UnderworldLayerY));
    }
  }
}
