using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
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
  private readonly TileObjectLiquidRuleRegistry _tileObjectRules;
  private readonly LiquidSettleSystem _settleSystem = new();

  public LiquidPropagationSystem(
    LiquidRuleRegistry? rules = null,
    TileDefinitionRegistry? tileDefinitions = null,
    TileObjectLiquidRuleRegistry? tileObjectRules = null)
  {
    _rules = rules ?? LiquidRuleRegistry.CreateDefault();
    _tileDefinitions = tileDefinitions ?? DefaultTileDefinitions;
    _tileObjectRules = tileObjectRules ?? TileObjectLiquidRuleRegistry.Empty;
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
    List<TileChangeCommand> tileCommands = new();
    LiquidSequenceAllocator sequenceAllocator = new(firstSequence);
    state.ObserveQueueLength(queue.Count);
    int maximumNodes = Math.Min(queue.Count, state.EffectiveTickBudget);
    long availablePairs = (long.MaxValue - firstSequence) / 2;
    if (maximumNodes > 0 && availablePairs == 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(firstSequence),
        "Liquid change sequence space is exhausted.");
    }

    if (availablePairs < maximumNodes)
    {
      maximumNodes = availablePairs > int.MaxValue ? int.MaxValue : (int)availablePairs;
    }

    IReadOnlyList<LiquidUpdateNode> nodes = queue.Drain(maximumNodes);
    for (int index = 0; index < nodes.Count; index++)
    {
      LiquidUpdateNode node = nodes[index];
      PipelineLiquidSourceComponent source = queue.TryGetSource(
        node,
        out PipelineLiquidSourceComponent pending)
        ? pending
        : ReadSource(world, node);
      queue.RemoveSource(node);
      if (source.Amount == 0 || source.Sequence == long.MaxValue || !Enum.IsDefined(source.Type))
      {
        continue;
      }

      AppendContactTileCommand(world, source, tileCommands);

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
      if (!sequenceAllocator.TryReserve(2, out long changeSequence))
      {
        throw new ArgumentOutOfRangeException(
          nameof(firstSequence),
          "Liquid change sequence space is exhausted.");
      }

      commands.Add(new PipelineLiquidChangeCommand(
        changeSequence,
        source.X,
        source.Y,
        remaining,
        (byte)source.Type));
      commands.Add(new PipelineLiquidChangeCommand(
        changeSequence + 1,
        targetX,
        targetY,
        targetAmount,
        (byte)source.Type));
      if (sequenceAllocator.NextSequence != long.MaxValue)
      {
        _ = queue.TryEnqueue(targetX, targetY, sequenceAllocator.NextSequence);
      }
    }

    return new LiquidPropagationResult(
      commands,
      tileCommands,
      sequenceAllocator.NextSequence,
      nodes.Count);
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

  private void AppendContactTileCommand(
    WorldGrid world,
    PipelineLiquidSourceComponent source,
    ICollection<TileChangeCommand> commands)
  {
    WorldTile tile = world.GetTile(source.X, source.Y);
    if (!tile.IsActive || !_tileDefinitions.TryGet(tile.Type, out TileDefinition definition))
    {
      return;
    }

    if (_tileObjectRules.HasRules(tile.Type))
    {
      AppendObjectRuleCommands(world, source, commands, tile);
      return;
    }

    bool destroysTile = source.Type == LiquidType.Lava
      ? definition.LavaDestroysTile
      : definition.WaterDestroysTile;
    if (!destroysTile)
    {
      return;
    }

    commands.Add(new TileChangeCommand(
      source.Sequence,
      source.X,
      source.Y,
      TileChangeKind.Kill,
      TileType: 0,
      PreserveLiquid: true));
  }

  private void AppendObjectRuleCommands(
    WorldGrid world,
    PipelineLiquidSourceComponent source,
    ICollection<TileChangeCommand> commands,
    WorldTile hitTile)
  {
    for (int width = 1; width <= 8; width++)
    {
      for (int height = 1; height <= 8; height++)
      {
        int originX = source.X - Modulo(hitTile.FrameX, width * 18) / 18;
        int originY = source.Y - Modulo(hitTile.FrameY, height * 18) / 18;
        if (!world.Contains(originX, originY) ||
            !world.Contains(originX + width - 1, originY + height - 1))
        {
          continue;
        }

        WorldTile origin = world.GetTile(originX, originY);
        if (!_tileObjectRules.TryGet(hitTile.Type, source.Type, origin.FrameX,
              out TileObjectLiquidRule rule) || rule.Width != width || rule.Height != height)
        {
          continue;
        }

        if (!rule.DestroysTile)
        {
          return;
        }

        for (int row = 0; row < rule.Height; row++)
        {
          for (int column = 0; column < rule.Width; column++)
          {
            int x = originX + column;
            int y = originY + row;
            WorldTile member = world.GetTile(x, y);
            if (!member.IsActive || member.Type != rule.TileType)
            {
              return;
            }

            if (source.Sequence > long.MaxValue - commands.Count)
            {
              return;
            }

            commands.Add(new TileChangeCommand(
              source.Sequence + commands.Count,
              x,
              y,
              TileChangeKind.Kill,
              TileType: 0,
              PreserveLiquid: true));
          }
        }

        return;
      }
    }
  }

  private static byte GetAmount(WorldGrid world, int x, int y)
  {
    return world.GetTile(x, y).LiquidAmount;
  }

  private static int Modulo(int value, int divisor)
  {
    int remainder = value % divisor;
    return remainder < 0 ? remainder + divisor : remainder;
  }
}

public sealed record LiquidPropagationResult(
  IReadOnlyList<PipelineLiquidChangeCommand> Commands,
  IReadOnlyList<TileChangeCommand> TileCommands,
  long NextSequence,
  int ProcessedCount);
