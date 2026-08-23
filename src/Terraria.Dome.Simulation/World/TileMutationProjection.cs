using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileMutationProjection
{
  public static WorldTile ApplyPending(
    WorldTile current,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(commands);
    List<TileChangeCommand> applicableCommands = new();
    foreach (TileChangeCommand command in commands)
    {
      if (command.X == x && command.Y == y)
      {
        applicableCommands.Add(command);
      }
    }

    applicableCommands.Sort(TileChangeCommandComparer.Instance);
    foreach (TileChangeCommand command in applicableCommands)
    {
      current = Apply(current, command);
    }

    return current;
  }

  public static WorldTile Apply(WorldTile current, TileChangeCommand command)
  {
    return command.Kind switch
    {
      TileChangeKind.Kill => CreateKilledTile(
        current,
        command.PreserveLiquid,
        command.FrameX,
        command.FrameY),
      TileChangeKind.SetInactive => current with { IsInactive = command.IsInactive },
      TileChangeKind.SetWall => current with { WallType = command.WallType },
      TileChangeKind.UpdateTileType => current with
      {
        IsActive = true,
        Type = command.TileType,
        FrameX = command.FrameX ?? current.FrameX,
        FrameY = command.FrameY ?? current.FrameY
      },
      TileChangeKind.UpdateTileShape => current with
      {
        IsHalfBrick = command.IsHalfBrick ?? current.IsHalfBrick,
        Slope = command.Slope ?? current.Slope
      },
      TileChangeKind.Place or TileChangeKind.PlaceTile => new WorldTile(
        IsActive: true,
        Type: command.TileType,
        FrameX: command.FrameX ?? 0,
        FrameY: command.FrameY ?? 0),
      _ => current
    };
  }

  private static WorldTile CreateKilledTile(
    WorldTile current,
    bool preserveLiquid,
    short? frameX,
    short? frameY)
  {
    WorldTile killed = preserveLiquid
      ? new WorldTile(
        IsActive: false,
        Type: 0,
        LiquidAmount: current.LiquidAmount,
        LiquidType: current.LiquidType)
      : default;
    return killed with
    {
      FrameX = frameX ?? killed.FrameX,
      FrameY = frameY ?? killed.FrameY
    };
  }

  private sealed class TileChangeCommandComparer : IComparer<TileChangeCommand>
  {
    public static readonly TileChangeCommandComparer Instance = new();

    public int Compare(TileChangeCommand first, TileChangeCommand second)
    {
      int sequenceComparison = first.Sequence.CompareTo(second.Sequence);
      return sequenceComparison != 0
        ? sequenceComparison
        : first.Kind.CompareTo(second.Kind);
    }
  }
}
