using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects.Placement;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct ProjectileWorldObjectPlacementCommand(
  long Sequence,
  Entity Projectile,
  Entity Owner,
  int OriginX,
  int OriginY,
  ushort ObjectType,
  int Style,
  int Direction,
  string SignText,
  long? ExpectedSectionVersion = null,
  ProjectileTombstoneReason TombstoneReason = ProjectileTombstoneReason.Expired)
{
  public WorldObjectPlacementRequest ToRequest()
  {
    return new(
      Sequence,
      Projectile,
      Owner,
      OriginX,
      OriginY,
      ObjectType,
      Style,
      Direction,
      SignText,
      ExpectedSectionVersion);
  }

  public bool IsValid(WorldGrid world)
  {
    return world is not null && ToRequest().Validate(world) ==
      WorldObjectPlacementFailureCode.None;
  }

  public WorldObjectPlacementFailureCode Validate(WorldGrid world)
  {
    return ToRequest().Validate(world);
  }
}
