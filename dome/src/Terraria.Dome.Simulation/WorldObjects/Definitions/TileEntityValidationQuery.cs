using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TileEntityValidationQuery
{
  public static bool IsValid(
    TileEntityIdentityComponent identity,
    TileEntityAnchorComponent anchor,
    WorldGrid world)
  {
    return identity.EntityId > 0 &&
      TileEntityDefinitionRegistry.TryGet(identity.Type, out _) &&
      world.Contains(anchor.TileX, anchor.TileY);
  }
}
