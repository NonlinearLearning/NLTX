using System;
using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public readonly record struct WorldObjectPlacementRequest(
  long Sequence,
  Entity Projectile,
  Entity Owner,
  int OriginX,
  int OriginY,
  ushort ObjectType,
  int Style,
  int Direction,
  string SignText,
  long? ExpectedSectionVersion = null)
{
  public const ushort SignObjectType = 85;
  public const int MaximumSignTextLength = 100;

  private static readonly WorldObjectPlacementDefinitionRegistry DefaultDefinitions =
    WorldObjectPlacementDefinitionRegistry.CreateVersion4Base();

  public bool IsShapeValid()
  {
    return Sequence >= 0 &&
      DefaultDefinitions.TryGet(ObjectType, out WorldObjectPlacementDefinition definition) &&
      definition.IsValidStyle(Style) &&
      definition.IsValidDirection(Direction) &&
      SignText is not null &&
      SignText.Length <= MaximumSignTextLength;
  }

  public WorldObjectPlacementFailureCode Validate(WorldGrid world)
  {
    return Validate(world, DefaultDefinitions);
  }

  public WorldObjectPlacementFailureCode Validate(
    WorldGrid world,
    WorldObjectPlacementDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (Sequence < 0)
    {
      return WorldObjectPlacementFailureCode.InvalidSequence;
    }

    if (world is null || !world.Contains(OriginX, OriginY))
    {
      return WorldObjectPlacementFailureCode.InvalidOrigin;
    }

    if (!definitions.TryGet(ObjectType, out WorldObjectPlacementDefinition definition))
    {
      return WorldObjectPlacementFailureCode.UnsupportedObject;
    }

    int lastX;
    int lastY;
    try
    {
      lastX = checked(OriginX + definition.Width - 1);
      lastY = checked(OriginY + definition.Height - 1);
    }
    catch (OverflowException)
    {
      return WorldObjectPlacementFailureCode.InvalidOrigin;
    }

    if (!world.Contains(lastX, lastY))
    {
      return WorldObjectPlacementFailureCode.InvalidOrigin;
    }

    if (!definition.IsValidStyle(Style))
    {
      return WorldObjectPlacementFailureCode.InvalidStyle;
    }

    if (!definition.IsValidDirection(Direction))
    {
      return WorldObjectPlacementFailureCode.InvalidDirection;
    }

    if (SignText is null || SignText.Length > MaximumSignTextLength)
    {
      return WorldObjectPlacementFailureCode.InvalidSignText;
    }

    return WorldObjectPlacementFailureCode.None;
  }
}
