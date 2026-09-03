using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkWorldSlice(
  string Name,
  int WorldId,
  int Width,
  int Height,
  int SpawnX,
  int SpawnY,
  byte GoodBiomeTileType,
  byte EvilBiomeTileType,
  byte BloodBiomeTileType,
  byte AnglerQuest,
  ushort SolarTowerShieldStrength,
  ushort VortexTowerShieldStrength,
  ushort NebulaTowerShieldStrength,
  ushort StardustTowerShieldStrength)
{
  public static NetworkWorldSlice From(
    WorldMetadata metadata,
    WorldJoinStateSnapshot joinState)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return new NetworkWorldSlice(
      metadata.Name,
      metadata.WorldId,
      metadata.Width,
      metadata.Height,
      metadata.SpawnX,
      metadata.SpawnY,
      joinState.GoodBiomeTileType,
      joinState.EvilBiomeTileType,
      joinState.BloodBiomeTileType,
      joinState.AnglerQuest,
      joinState.SolarTowerShieldStrength,
      joinState.VortexTowerShieldStrength,
      joinState.NebulaTowerShieldStrength,
      joinState.StardustTowerShieldStrength);
  }
}
