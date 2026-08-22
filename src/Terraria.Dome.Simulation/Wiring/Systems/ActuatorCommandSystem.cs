using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class ActuatorCommandSystem
{
  public IReadOnlyList<TileChangeCommand> CreateCommands(
    WorldGrid world,
    ActuatorComponent actuator,
    MechanismActivationCommand activation,
    long firstSequence,
    Func<int, int, bool>? canKillTileQuery = null,
    WorldMetadata? worldMetadata = null,
    WorldProgressionState? progression = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(actuator);
    if (activation.MechanismId != actuator.MechanismId || firstSequence < 0)
    {
      return [];
    }

    List<TileChangeCommand> commands = new(actuator.Tiles.Count);
    for (int index = 0; index < actuator.Tiles.Count; index++)
    {
      WiringTileCoordinate tile = actuator.Tiles[index];
      if (!TryCreateCommand(
            world,
            tile,
            checked(firstSequence + index),
            canKillTileQuery,
            worldMetadata,
            progression,
            out TileChangeCommand command))
      {
        return [];
      }

      commands.Add(command);
    }

    if (commands.Count > 0)
    {
      actuator.SetEnabled(!commands[0].IsInactive);
    }

    return commands;
  }

  private static bool TryCreateCommand(
    WorldGrid world,
    WiringTileCoordinate coordinates,
    long sequence,
    Func<int, int, bool>? canKillTileQuery,
    WorldMetadata? worldMetadata,
    WorldProgressionState? progression,
    out TileChangeCommand command)
  {
    command = default;
    if (!world.Contains(coordinates.X, coordinates.Y))
    {
      return false;
    }

    WorldTile tile = world.GetTile(coordinates.X, coordinates.Y);
    if (!tile.IsActive || !tile.IsActuated)
    {
      return false;
    }

    if (!tile.IsInactive && !CanDeactivate(
          world,
          coordinates,
          tile,
          canKillTileQuery,
          worldMetadata,
          progression))
    {
      return false;
    }

    command = new TileChangeCommand(
      sequence,
      coordinates.X,
      coordinates.Y,
      TileChangeKind.SetInactive,
      tile.Type,
      IsInactive: !tile.IsInactive);
    return true;
  }

  private static bool CanDeactivate(
    WorldGrid world,
    WiringTileCoordinate coordinates,
    WorldTile tile,
    Func<int, int, bool>? canKillTileQuery,
    WorldMetadata? worldMetadata,
    WorldProgressionState? progression)
  {
    if (!LegacyActuatorTileDefinitionRuleSystem.TryGet(
          tile.Type,
          out bool isSolid,
          out bool isNotReallySolid) || coordinates.Y <= 0)
    {
      return false;
    }

    bool isType226 = tile.Type == 226;
    bool belowWorldSurface = isType226 &&
      (worldMetadata?.WorldSurface is not double worldSurface || coordinates.Y > worldSurface);
    if (isType226 && worldMetadata?.WorldSurface is null)
    {
      return false;
    }

    WorldTile tileAbove = world.GetTile(coordinates.X, coordinates.Y - 1);
    bool tileAbovePreventsActuation = tileAbove.IsActive &&
      LegacyActuationProtectionRuleSystem.PreventsActuationUnder(tileAbove.Type);
    bool canKillTile = tileAbove.IsActive && !tileAbovePreventsActuation &&
      canKillTileQuery?.Invoke(coordinates.X, coordinates.Y) == true;
    return ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: tile.IsActive,
      isActuated: tile.IsActuated,
      tileType: tile.Type,
      isSolid,
      isNotReallySolid,
      isType226,
      belowWorldSurface,
      defeatedPlantera: progression?.DefeatedPlantera == true,
      tileAboveIsActive: tileAbove.IsActive,
      tileAbovePreventsActuation,
      canKillTile);
  }
}
