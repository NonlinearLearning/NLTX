using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyActuatorTileDefinitionRuleSystem
{
  private static readonly TileDefinitionRegistry TileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();

  public static bool TryGet(ushort tileType, out bool isSolid, out bool isNotReallySolid)
  {
    isSolid = TileDefinitions.TryGet(tileType, out TileDefinition definition) &&
      definition.BlocksLiquid;
    isNotReallySolid = tileType is 10 or 387 or 388;
    return isSolid || isNotReallySolid || IsSpecialNonActuated(tileType);
  }

  private static bool IsSpecialNonActuated(ushort tileType)
  {
    return ActuatorDeactivationRuleSystem.RegisterSpecialNonActuatedDefaults().Contains(tileType);
  }
}
