using System;
using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public static class SignObjectPlacementPlanFactory
{
  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();
  private static readonly WorldObjectPlacementDefinitionRegistry DefaultDefinitions =
    WorldObjectPlacementDefinitionRegistry.CreateVersion4Base();

  public static bool TryCreate(
    WorldGrid world,
    WorldObjectPlacementRequest request,
    out WorldObjectPlacementPlan plan,
    out WorldObjectPlacementFailureCode failureCode,
    WorldObjectPlacementDefinitionRegistry? definitions = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    definitions ??= DefaultDefinitions;
    failureCode = request.Validate(world, definitions);
    if (failureCode != WorldObjectPlacementFailureCode.None)
    {
      plan = null!;
      return false;
    }

    if (!definitions.TryGet(
          request.ObjectType,
          out WorldObjectPlacementDefinition definition))
    {
      plan = null!;
      failureCode = WorldObjectPlacementFailureCode.UnsupportedObject;
      return false;
    }

    List<WorldObjectTileMutation> footprint = new(definition.Width * definition.Height);
    Dictionary<WorldSectionCoordinates, long> sections = new();
    for (int row = 0; row < definition.Height; row++)
    {
      for (int column = 0; column < definition.Width; column++)
      {
        int x = request.OriginX + column;
        int y = request.OriginY + row;
        if (!world.Contains(x, y))
        {
          plan = null!;
          failureCode = WorldObjectPlacementFailureCode.InvalidOrigin;
          return false;
        }

        WorldTile expected = world.GetTile(x, y);
        if (expected.IsActive)
        {
          plan = null!;
          failureCode = WorldObjectPlacementFailureCode.OccupiedTile;
          return false;
        }

        if (!definition.TryGetFrame(
              request.Style,
              column,
              row,
              out short frameX,
              out short frameY))
        {
          plan = null!;
          failureCode = WorldObjectPlacementFailureCode.InvalidFrame;
          return false;
        }

        WorldSectionCoordinates section = world.GetSectionCoordinates(x, y);
        sections.TryAdd(section, world.GetSectionVersion(section));
        WorldTile replacement = new(
          IsActive: true,
          Type: request.ObjectType,
          FrameX: frameX,
          FrameY: frameY);
        footprint.Add(new WorldObjectTileMutation(x, y, expected, replacement));
      }
    }

    if (!definition.HasValidAnchor(
          world,
          request.OriginX,
          request.OriginY,
          DefaultTileDefinitions))
    {
      plan = null!;
      failureCode = WorldObjectPlacementFailureCode.MissingSupport;
      return false;
    }

    int supportY = checked(request.OriginY + definition.Height);
    for (int column = 0; column < definition.Width; column++)
    {
      int supportX = checked(request.OriginX + column);
      WorldSectionCoordinates supportSection = world.GetSectionCoordinates(supportX, supportY);
      sections.TryAdd(supportSection, world.GetSectionVersion(supportSection));
    }

    plan = new WorldObjectPlacementPlan(request, footprint, sections);
    return true;
  }
}
