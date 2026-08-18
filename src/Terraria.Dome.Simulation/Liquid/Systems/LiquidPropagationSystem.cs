using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Liquid.Definitions;
using Terraria.Dome.Simulation.WorldModel.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidChangeCommand = Terraria.Dome.Simulation.Commands.LiquidChangeCommand;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidPropagationSystem
{
  private static readonly TileDefinitionRegistry DefaultTileDefinitions =
    TileDefinitionRegistry.CreateVersion4Base();
  private readonly LiquidRuleRegistry _rules;
  private readonly TileDefinitionRegistry _tileDefinitions;
  private readonly LiquidSettleSystem _settleSystem = new();

  public LiquidPropagationSystem(
    LiquidRuleRegistry? rules = null,
    TileDefinitionRegistry? tileDefinitions = null)
  {
    _rules = rules ?? LiquidRuleRegistry.CreateDefault();
    _tileDefinitions = tileDefinitions ?? DefaultTileDefinitions;
  }

  public LiquidPropagationResult Advance(
    WorldGrid world,
    LiquidUpdateQueueComponent queue,
    LiquidWorldStateComponent state,
    long firstSequence)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(queue);
    ArgumentNullException.ThrowIfNull(state);
    if (firstSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(firstSequence));
    }

    List<PipelineLiquidChangeCommand> commands = new();
    long nextSequence = firstSequence;
    IReadOnlyList<LiquidUpdateNode> nodes = queue.Drain(state.TickBudget);
    for (int index = 0; index < nodes.Count; index++)
    {
      LiquidUpdateNode node = nodes[index];
      PipelineLiquidSourceComponent source = queue.TryGetSource(
        node,
        out PipelineLiquidSourceComponent pending)
        ? pending
        : ReadSource(world, node);
      queue.RemoveSource(node);
      if (source.Amount == 0 || !Enum.IsDefined(source.Type))
      {
        continue;
      }

      LiquidRuleDefinition rule = _rules.Get(source.Type);
      int transfer = Math.Min(source.Amount, rule.TransferAmount);
      if (!TryFindTarget(world, source, transfer, out int targetX, out int targetY,
            out byte targetAmount))
      {
        _ = _settleSystem.Requeue(queue, node);
        continue;
      }

      queue.ResetRetryCount(source.X, source.Y);
      byte remaining = (byte)(source.Amount - (targetAmount - GetAmount(world, targetX, targetY)));
      commands.Add(new PipelineLiquidChangeCommand(
        nextSequence++,
        source.X,
        source.Y,
        remaining,
        (byte)source.Type));
      commands.Add(new PipelineLiquidChangeCommand(
        nextSequence++,
        targetX,
        targetY,
        targetAmount,
        (byte)source.Type));
      _ = queue.TryEnqueue(targetX, targetY, nextSequence);
    }

    return new LiquidPropagationResult(commands, nextSequence, nodes.Count);
  }

  private static PipelineLiquidSourceComponent ReadSource(WorldGrid world, LiquidUpdateNode node)
  {
    WorldTile tile = world.GetTile(node.X, node.Y);
    return new PipelineLiquidSourceComponent(
      node.X,
      node.Y,
      tile.LiquidAmount,
      (LiquidType)tile.LiquidType,
      node.Sequence);
  }

  private bool TryFindTarget(
    WorldGrid world,
    PipelineLiquidSourceComponent source,
    int transfer,
    out int targetX,
    out int targetY,
    out byte targetAmount)
  {
    (int X, int Y)[] candidates =
    [
      (source.X, source.Y - 1),
      (source.X - 1, source.Y),
      (source.X + 1, source.Y)
    ];
    for (int index = 0; index < candidates.Length; index++)
    {
      (int x, int y) = candidates[index];
      if (!world.Contains(x, y))
      {
        continue;
      }

      WorldTile tile = world.GetTile(x, y);
      if (tile.IsActive &&
          (!_tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
           definition.BlocksLiquid && !definition.IsPlatform))
      {
        continue;
      }

      if (tile.LiquidAmount != 0 && tile.LiquidType != (byte)source.Type)
      {
        continue;
      }

      int available = byte.MaxValue - tile.LiquidAmount;
      int moved = Math.Min(transfer, available);
      if (moved == 0)
      {
        continue;
      }

      targetX = x;
      targetY = y;
      targetAmount = (byte)(tile.LiquidAmount + moved);
      return true;
    }

    targetX = 0;
    targetY = 0;
    targetAmount = 0;
    return false;
  }

  private static byte GetAmount(WorldGrid world, int x, int y)
  {
    return world.GetTile(x, y).LiquidAmount;
  }
}

public sealed record LiquidPropagationResult(
  IReadOnlyList<PipelineLiquidChangeCommand> Commands,
  long NextSequence,
  int ProcessedCount);
