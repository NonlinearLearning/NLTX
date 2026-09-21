using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Validation;

public enum TileInteractionRejection
{
  None,
  UnsupportedAction,
  OutOfWorld,
  SectionNotVisible,
  OutOfRange,
  ActionBudgetExceeded,
  InvalidTileType
}

public readonly record struct TileInteractionValidation(
  bool IsAccepted,
  TileInteractionRejection Rejection,
  TileChangeCommand? Command)
{
  public static TileInteractionValidation Reject(TileInteractionRejection rejection)
  {
    return new TileInteractionValidation(false, rejection, null);
  }
}

public sealed class TileInteractionValidator
{
  public const int MaximumActionsPerTick = 32;
  public const float MaximumInteractionRange = 12.0f;

  public TileInteractionValidation Validate(
    TileManipulationIntent intent,
    PlayerSnapshot player,
    WorldGrid world,
    IReadOnlySet<WorldSectionCoordinates> visibleSections,
    int actionsThisTick,
    long sequence)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(visibleSections);
    if (intent.Action is not TileManipulationAction.KillTile and
      not TileManipulationAction.PlaceTile)
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.UnsupportedAction);
    }

    if (intent.X < 0 || intent.X >= world.Width || intent.Y < 0 || intent.Y >= world.Height)
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.OutOfWorld);
    }

    WorldSectionCoordinates section = world.GetSectionCoordinates(intent.X, intent.Y);
    if (!visibleSections.Contains(section))
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.SectionNotVisible);
    }

    float deltaX = intent.X - player.Position.X;
    float deltaY = intent.Y - player.Position.Y;
    if (deltaX * deltaX + deltaY * deltaY >
      MaximumInteractionRange * MaximumInteractionRange)
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.OutOfRange);
    }

    if (actionsThisTick >= MaximumActionsPerTick)
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.ActionBudgetExceeded);
    }

    if (intent.Action == TileManipulationAction.PlaceTile && intent.TileType < 0)
    {
      return TileInteractionValidation.Reject(TileInteractionRejection.InvalidTileType);
    }

    TileChangeKind kind = intent.Action == TileManipulationAction.KillTile
      ? TileChangeKind.Kill
      : TileChangeKind.Place;
    TileChangeCommand command = new(sequence, intent.X, intent.Y, kind, (ushort)intent.TileType);
    return new TileInteractionValidation(true, TileInteractionRejection.None, command);
  }
}
