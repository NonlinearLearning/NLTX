using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidTransferSystem
{
  public LiquidTransferResult CreateChanges(
    WorldGrid world,
    IReadOnlyCollection<LiquidTransferCommand> transfers,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(transfers);
    if (firstSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(firstSequence));
    }

    List<LiquidTransferCommand> ordered = [.. transfers];
    ordered.Sort(LiquidTransferCommandComparer.Instance);
    Dictionary<(int X, int Y), WorldTile> tiles = new();
    List<LiquidChangeCommand> changes = new();
    long nextSequence = firstSequence;
    for (int index = 0; index < ordered.Count; index++)
    {
      LiquidTransferCommand transfer = ordered[index];
      if (!TryMove(world, tiles, transfer, out WorldTile source, out WorldTile target))
      {
        continue;
      }

      tiles[(transfer.SourceX, transfer.SourceY)] = source;
      tiles[(transfer.TargetX, transfer.TargetY)] = target;
      changes.Add(new LiquidChangeCommand(
        nextSequence++,
        transfer.SourceX,
        transfer.SourceY,
        source.LiquidAmount,
        source.LiquidType));
      changes.Add(new LiquidChangeCommand(
        nextSequence++,
        transfer.TargetX,
        transfer.TargetY,
        target.LiquidAmount,
        target.LiquidType));
    }

    return new LiquidTransferResult(changes, nextSequence);
  }

  private static bool TryMove(
    WorldGrid world,
    IReadOnlyDictionary<(int X, int Y), WorldTile> tiles,
    LiquidTransferCommand transfer,
    out WorldTile source,
    out WorldTile target)
  {
    source = default;
    target = default;
    if (transfer.Sequence < 0 || transfer.Amount == 0 ||
        transfer.Type > (byte)LiquidType.Shimmer ||
        transfer.SourceX == transfer.TargetX && transfer.SourceY == transfer.TargetY ||
        !world.Contains(transfer.SourceX, transfer.SourceY) ||
        !world.Contains(transfer.TargetX, transfer.TargetY))
    {
      return false;
    }

    source = tiles.TryGetValue((transfer.SourceX, transfer.SourceY), out WorldTile sourceTile)
      ? sourceTile
      : world.GetTile(transfer.SourceX, transfer.SourceY);
    target = tiles.TryGetValue((transfer.TargetX, transfer.TargetY), out WorldTile targetTile)
      ? targetTile
      : world.GetTile(transfer.TargetX, transfer.TargetY);
    if (source.LiquidAmount == 0 || source.LiquidType != transfer.Type ||
        target.LiquidAmount != 0 && target.LiquidType != transfer.Type)
    {
      return false;
    }

    byte amount = (byte)Math.Min(
      transfer.Amount,
      Math.Min(source.LiquidAmount, byte.MaxValue - target.LiquidAmount));
    if (amount == 0)
    {
      return false;
    }

    source = source with { LiquidAmount = (byte)(source.LiquidAmount - amount) };
    target = target with { LiquidAmount = (byte)(target.LiquidAmount + amount), LiquidType = transfer.Type };
    return true;
  }

  private sealed class LiquidTransferCommandComparer : IComparer<LiquidTransferCommand>
  {
    public static readonly LiquidTransferCommandComparer Instance = new();

    public int Compare(LiquidTransferCommand first, LiquidTransferCommand second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int sourceX = first.SourceX.CompareTo(second.SourceX);
      if (sourceX != 0)
      {
        return sourceX;
      }

      int sourceY = first.SourceY.CompareTo(second.SourceY);
      if (sourceY != 0)
      {
        return sourceY;
      }

      int targetX = first.TargetX.CompareTo(second.TargetX);
      return targetX != 0 ? targetX : first.TargetY.CompareTo(second.TargetY);
    }
  }
}

public sealed record LiquidTransferResult(
  IReadOnlyList<LiquidChangeCommand> Changes,
  long NextSequence);
