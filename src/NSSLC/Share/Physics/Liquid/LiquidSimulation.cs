using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria.Physics;
using Terraria.World;

namespace Terraria.Liquid;

/// <summary>
/// Owns the deterministic tile-level liquid simulation: the settle queue, the per-pass
/// deduplication table and the bounded step that flows and equalizes liquid.
/// </summary>
/// <remarks>
/// This type holds the liquid algorithm only. It never touches the ECS, never publishes
/// mutations and never advances the world revision on its own authority: the caller supplies the
/// tile storage and receives every changed coordinate back as a produced delta, so the single
/// world write boundary stays in <see cref="WorldRuntime"/>.
///
/// Threading: not thread safe. The owning world serializes all calls under its own lock.
/// </remarks>
internal sealed class LiquidSimulation
{
  /// <summary>
  /// 每 tick 的液体操作上限。按实测每操作约 4.8 µs 折算，350 次约 1.7 ms，即 16.67 ms
  /// tick 预算的约 10%，给其他阶段留出余量；液体在稳态下由“液面差 ≤1 不搬运”收敛，
  /// 正常情况下远达不到该上限。
  /// </summary>
  internal const int DefaultOperationBudget = 350;
  internal const int MaximumQueueEntries = 75_000;

  private readonly Queue<int> _settleQueue = [];
  private readonly Queue<int> _urgentQueue = [];
  private readonly bool[] _queued;
  private readonly bool[] _urgent;
  private readonly int[] _processedPasses;
  private int _pass;
  private bool _passActive;

  internal LiquidSimulation(int tileCount)
  {
    if (tileCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileCount));
    }

    _queued = new bool[tileCount];
    _urgent = new bool[tileCount];
    _processedPasses = new int[tileCount];
  }

  /// <summary>True when the queue still holds coordinates for a later pass.</summary>
  internal bool HasQueuedWork => _settleQueue.Count > 0 || _urgentQueue.Count > 0;

  internal void Reset()
  {
    _settleQueue.Clear();
    _urgentQueue.Clear();
    Array.Clear(_queued);
    Array.Clear(_urgent);
    Array.Clear(_processedPasses);
    _pass = 0;
    _passActive = false;
  }

  /// <summary>Queues a coordinate whose liquid may need to move.</summary>
  internal void Enqueue(int index, bool urgent)
  {
    if (_passActive && _processedPasses[index] == _pass)
    {
      return;
    }

    if (_queued[index])
    {
      if (urgent && !_urgent[index])
      {
        _urgent[index] = true;
        _urgentQueue.Enqueue(index);
      }

      return;
    }

    if (_settleQueue.Count + _urgentQueue.Count >= MaximumQueueEntries)
    {
      if (!urgent || _settleQueue.Count == 0)
      {
        return;
      }

      int evictedIndex = _settleQueue.Dequeue();
      _queued[evictedIndex] = false;
      _urgent[evictedIndex] = false;
    }

    _queued[index] = true;
    _urgent[index] = urgent;
    if (urgent)
    {
      _urgentQueue.Enqueue(index);
      return;
    }

    _settleQueue.Enqueue(index);
  }

  /// <summary>
  /// Runs one bounded liquid pass over the queue and returns the coordinates it changed.
  /// </summary>
  /// <remarks>
  /// The caller must supply a storage whose writes are already staged or otherwise owned by it;
  /// this method mutates tiles in place and reports every changed coordinate exactly once.
  /// </remarks>
  internal LiquidStepOutcome Step(
      LiquidTileStorage storage,
      int operationBudget,
      List<LiquidTileChange> changed)
  {
    ArgumentNullException.ThrowIfNull(storage);
    ArgumentNullException.ThrowIfNull(changed);
    if (operationBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(operationBudget));
    }

    int simulationPass = NextPass();
    _passActive = true;
    int processed = 0;
    int operations = 0;
    bool revisionAdvanced = false;
    try
    {
      // Count blocked and dry entries too, so a stagnant queue cannot consume an entire tick.
      while (processed < operationBudget &&
          (_urgentQueue.Count > 0 || _settleQueue.Count > 0))
      {
        int sourceIndex = _urgentQueue.Count > 0
            ? _urgentQueue.Dequeue()
            : _settleQueue.Dequeue();
        processed++;
        if (!_queued[sourceIndex])
        {
          continue;
        }

        _queued[sourceIndex] = false;
        _urgent[sourceIndex] = false;
        if (_processedPasses[sourceIndex] == simulationPass)
        {
          continue;
        }

        _processedPasses[sourceIndex] = simulationPass;
        WorldTileCoordinate sourceCoordinate = storage.ToCoordinate(sourceIndex);
        WorldTileState source = storage.Read(sourceCoordinate);
        if (!source.HasLiquid || LiquidBlockingRules.BlocksLiquid(source) ||
            source.LiquidAmount == 0)
        {
          continue;
        }

        // 只有真正能移动的液体才进入搬运逻辑：否则本格既不产生变化，也不该占用预算。
        if (!NeedsFlow(storage, sourceCoordinate))
        {
          continue;
        }

        bool sourceChanged = false;
        if (sourceCoordinate.Y + 1 < storage.Height)
        {
          sourceChanged = TryFlow(
              storage,
              sourceCoordinate,
              new WorldTileCoordinate(sourceCoordinate.X, sourceCoordinate.Y + 1),
              maxTransfer: byte.MaxValue,
              ref revisionAdvanced,
              changed);
        }

        WorldTileState remaining = storage.Read(sourceCoordinate);
        if (remaining.HasLiquid &&
            TryEqualizeHorizontally(
                storage,
                sourceCoordinate,
                remaining,
                ref revisionAdvanced,
                changed))
        {
          sourceChanged = true;
        }

        if (sourceChanged)
        {
          operations++;
        }
      }
    }
    finally
    {
      _passActive = false;
    }

    return new LiquidStepOutcome(processed, operations, revisionAdvanced);
  }

  /// <summary>Queues a coordinate and its four direct neighbours.</summary>
  internal void EnqueueWithNeighbors(LiquidTileStorage storage, WorldTileCoordinate coordinate,
      bool urgent = false)
  {
    EnqueueWithNeighborsCore(storage, coordinate, urgent);
  }

  /// <summary>
  /// Seeds the queue with every liquid tile that is not yet settled after a world load.
  /// </summary>
  internal void SeedUnsettled(LiquidTileStorage storage)
  {
    ArgumentNullException.ThrowIfNull(storage);
    for (int y = storage.Height - 2; y > 0; y--)
    {
      for (int x = 1; x < storage.Width - 1; x++)
      {
        WorldTileCoordinate coordinate = new(x, y);
        WorldTileState tile = storage.Read(coordinate);
        if (!tile.HasLiquid || LiquidBlockingRules.BlocksLiquid(tile))
        {
          continue;
        }

        if (IsUnsettledLiquidNeighbor(storage, x, y, x - 1, y, tile) ||
            IsUnsettledLiquidNeighbor(storage, x, y, x + 1, y, tile) ||
            IsUnsettledLiquidNeighbor(storage, x, y, x, y - 1, tile) ||
            IsUnsettledLiquidNeighbor(storage, x, y, x, y + 1, tile))
        {
          Enqueue(storage.ToIndex(new WorldTileCoordinate(x, y)), urgent: false);
        }
      }
    }
  }

  /// <summary>Notifies the queue that a tile was written without going through the algorithm.</summary>
  internal void NotifyTileWritten(LiquidTileStorage storage, WorldTileCoordinate coordinate)
  {
    EnqueueWithNeighborsCore(storage, coordinate, urgent: true);
  }

  /// <summary>
  /// 判断该格当前是否存在可发生的液体移动：下方可容纳，或水平方向液面差 ≥2。
  /// </summary>
  /// <remarks>
  /// 只有返回 true 的格子才值得入队和处理。液面差 ≤1 的格子即使参与均衡也不会产生搬运，
  /// 而搬运算作变化又会再次触发邻居入队——正是这一点让队列曾经永远无法收敛。
  /// </remarks>
  private static bool NeedsFlow(LiquidTileStorage storage, WorldTileCoordinate coordinate)
  {
    WorldTileState source = storage.Read(coordinate);
    if (!source.HasLiquid || source.LiquidAmount == 0 || LiquidBlockingRules.BlocksLiquid(source))
    {
      return false;
    }

    if (coordinate.Y + 1 < storage.Height)
    {
      WorldTileState below = storage.Read(
          new WorldTileCoordinate(coordinate.X, coordinate.Y + 1));
      if (CanReceiveLiquid(source, below))
      {
        return true;
      }
    }

    if (coordinate.X > 0 &&
        HorizontalGap(source, storage.Read(new WorldTileCoordinate(coordinate.X - 1,
            coordinate.Y))) >= 2)
    {
      return true;
    }

    return coordinate.X + 1 < storage.Width &&
        HorizontalGap(source, storage.Read(new WorldTileCoordinate(coordinate.X + 1,
            coordinate.Y))) >= 2;
  }

  private static bool CanReceiveLiquid(WorldTileState source, WorldTileState target) =>
      !LiquidBlockingRules.BlocksLiquid(target) &&
      (!target.HasLiquid ||
          (target.Liquid == source.Liquid && target.LiquidAmount < byte.MaxValue));

  /// <summary>相邻两格的液面差；不能共享液体的相邻格返回 0。</summary>
  private static int HorizontalGap(WorldTileState source, WorldTileState neighbor)
  {
    if (!CanShareHorizontally(neighbor, source))
    {
      return 0;
    }

    int neighborAmount = neighbor.HasLiquid ? neighbor.LiquidAmount : 0;
    return Math.Abs(source.LiquidAmount - neighborAmount);
  }

  private void EnqueueWithNeighborsCore(LiquidTileStorage storage, WorldTileCoordinate coordinate,
      bool urgent)
  {
    // 只把“此刻确实能移动”的格子放进队列：液面稳定的区域不入队，队列才能真正排空。
    EnqueueIfNeeded(storage, coordinate, urgent);
    if (coordinate.X > 0)
    {
      EnqueueIfNeeded(storage,
          new WorldTileCoordinate(coordinate.X - 1, coordinate.Y), urgent);
    }

    if (coordinate.X + 1 < storage.Width)
    {
      EnqueueIfNeeded(storage,
          new WorldTileCoordinate(coordinate.X + 1, coordinate.Y), urgent);
    }

    if (coordinate.Y > 0)
    {
      EnqueueIfNeeded(storage,
          new WorldTileCoordinate(coordinate.X, coordinate.Y - 1), urgent);
    }

    if (coordinate.Y + 1 < storage.Height)
    {
      EnqueueIfNeeded(storage,
          new WorldTileCoordinate(coordinate.X, coordinate.Y + 1), urgent);
    }
  }

  private void EnqueueIfNeeded(LiquidTileStorage storage, WorldTileCoordinate coordinate,
      bool urgent)
  {
    if (NeedsFlow(storage, coordinate))
    {
      Enqueue(storage.ToIndex(coordinate), urgent);
    }
  }

  private bool TryFlow(
      LiquidTileStorage storage,
      WorldTileCoordinate sourceCoordinate,
      WorldTileCoordinate targetCoordinate,
      int maxTransfer,
      ref bool revisionAdvanced,
      List<LiquidTileChange> changed)
  {
    WorldTileState source = storage.Read(sourceCoordinate);
    WorldTileState target = storage.Read(targetCoordinate);
    if (!source.HasLiquid || LiquidBlockingRules.BlocksLiquid(target) ||
        (target.HasLiquid && target.Liquid != source.Liquid))
    {
      return false;
    }

    int capacity = byte.MaxValue - (target.HasLiquid ? target.LiquidAmount : 0);
    int amount = Math.Min(Math.Min(source.LiquidAmount, capacity), maxTransfer);
    if (amount <= 0)
    {
      return false;
    }

    revisionAdvanced = true;

    bool retainFullSource = amount == 1 && source.LiquidAmount == byte.MaxValue;
    byte nextSourceAmount = retainFullSource
        ? source.LiquidAmount
        : (byte)(source.LiquidAmount - amount);
    byte nextTargetAmount = (byte)((target.HasLiquid ? target.LiquidAmount : 0) + amount);
    WorldTileState nextSource = source with
    {
      Liquid = nextSourceAmount == 0 ? LiquidContactFlags.None : source.Liquid,
      LiquidAmount = nextSourceAmount
    };
    WorldTileState nextTarget = target with
    {
      Liquid = source.Liquid,
      LiquidAmount = nextTargetAmount
    };
    storage.Write(sourceCoordinate, nextSource);
    storage.Write(targetCoordinate, nextTarget);
    changed.Add(new LiquidTileChange(sourceCoordinate, source, nextSource));
    changed.Add(new LiquidTileChange(targetCoordinate, target, nextTarget));
    EnqueueWithNeighborsCore(storage, sourceCoordinate, urgent: false);
    EnqueueWithNeighborsCore(storage, targetCoordinate, urgent: false);
    return true;
  }

  private bool TryEqualizeHorizontally(
      LiquidTileStorage storage,
      WorldTileCoordinate sourceCoordinate,
      WorldTileState source,
      ref bool revisionAdvanced,
      List<LiquidTileChange> changed)
  {
    bool hasLeft = false;
    bool hasRight = false;
    WorldTileCoordinate leftCoordinate = default;
    WorldTileCoordinate rightCoordinate = default;
    WorldTileState left = default;
    WorldTileState right = default;
    if (sourceCoordinate.X > 0)
    {
      leftCoordinate = new WorldTileCoordinate(sourceCoordinate.X - 1, sourceCoordinate.Y);
      left = storage.Read(leftCoordinate);
      hasLeft = CanShareHorizontally(left, source);
    }

    if (sourceCoordinate.X + 1 < storage.Width)
    {
      rightCoordinate = new WorldTileCoordinate(sourceCoordinate.X + 1, sourceCoordinate.Y);
      right = storage.Read(rightCoordinate);
      hasRight = CanShareHorizontally(right, source);
    }

    int participantCount = 1 + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);
    if (participantCount == 1)
    {
      return false;
    }

    // 液面差 ≤1 时不做搬运。总额除以参与数会产生余数，而余数按“源优先”分配，于是相邻两格
    // 的 1 单位差会在两格之间来回倒手：每次倒手都算作变化，又触发邻居入队，队列永远清不空。
    // 实测这是液体每 tick 恒定跑满预算（约 2600 次操作、12–17 ms）的直接原因。
    int minAmount = source.LiquidAmount;
    int maxAmount = source.LiquidAmount;
    if (hasLeft)
    {
      int leftAmount = left.HasLiquid ? left.LiquidAmount : 0;
      minAmount = Math.Min(minAmount, leftAmount);
      maxAmount = Math.Max(maxAmount, leftAmount);
    }

    if (hasRight)
    {
      int rightAmount = right.HasLiquid ? right.LiquidAmount : 0;
      minAmount = Math.Min(minAmount, rightAmount);
      maxAmount = Math.Max(maxAmount, rightAmount);
    }

    if (maxAmount - minAmount <= 1)
    {
      return false;
    }

    int total = source.LiquidAmount;
    if (hasLeft)
    {
      total += left.HasLiquid ? left.LiquidAmount : 0;
    }
    if (hasRight)
    {
      total += right.HasLiquid ? right.LiquidAmount : 0;
    }

    int baseAmount = total / participantCount;
    int remainder = total % participantCount;
    byte sourceAmount = checked((byte)(baseAmount + (remainder > 0 ? 1 : 0)));
    if (remainder > 0)
    {
      remainder--;
    }

    WorldTileState nextSource = WithLiquidAmount(source, sourceAmount);
    WorldTileState nextLeft = left;
    WorldTileState nextRight = right;
    if (hasLeft)
    {
      byte amount = checked((byte)(baseAmount + (remainder > 0 ? 1 : 0)));
      if (remainder > 0)
      {
        remainder--;
      }
      nextLeft = WithLiquidAmount(left, amount, source.Liquid);
    }
    if (hasRight)
    {
      byte amount = checked((byte)(baseAmount + (remainder > 0 ? 1 : 0)));
      nextRight = WithLiquidAmount(right, amount, source.Liquid);
    }

    if (nextSource == source && (!hasLeft || nextLeft == left) &&
        (!hasRight || nextRight == right))
    {
      return false;
    }

    revisionAdvanced = true;

    if (nextSource != source)
    {
      storage.Write(sourceCoordinate, nextSource);
      changed.Add(new LiquidTileChange(sourceCoordinate, source, nextSource));
    }

    if (hasLeft && nextLeft != left)
    {
      storage.Write(leftCoordinate, nextLeft);
      changed.Add(new LiquidTileChange(leftCoordinate, left, nextLeft));
    }

    if (hasRight && nextRight != right)
    {
      storage.Write(rightCoordinate, nextRight);
      changed.Add(new LiquidTileChange(rightCoordinate, right, nextRight));
    }

    EnqueueWithNeighborsCore(storage, sourceCoordinate, urgent: false);
    if (hasLeft)
    {
      EnqueueWithNeighborsCore(storage, leftCoordinate, urgent: false);
    }

    if (hasRight)
    {
      EnqueueWithNeighborsCore(storage, rightCoordinate, urgent: false);
    }

    return true;
  }

  private static bool CanShareHorizontally(WorldTileState tile, WorldTileState source)
  {
    return !LiquidBlockingRules.BlocksLiquid(tile) &&
        (!tile.HasLiquid || tile.Liquid == source.Liquid);
  }

  private static WorldTileState WithLiquidAmount(
      WorldTileState tile,
      byte amount,
      LiquidContactFlags liquid = LiquidContactFlags.None)
  {
    LiquidContactFlags type = amount == 0
        ? LiquidContactFlags.None
        : liquid == LiquidContactFlags.None ? tile.Liquid : liquid;
    return tile with { Liquid = type, LiquidAmount = amount };
  }

  private int NextPass()
  {
    if (_pass == int.MaxValue)
    {
      Array.Clear(_processedPasses);
      _pass = 1;
      return _pass;
    }

    return ++_pass;
  }

  private bool IsUnsettledLiquidNeighbor(
      LiquidTileStorage storage,
      int sourceX,
      int sourceY,
      int neighborX,
      int neighborY,
      WorldTileState source)
  {
    WorldTileState neighbor = storage.Read(new WorldTileCoordinate(neighborX, neighborY));
    if (LiquidBlockingRules.BlocksLiquid(neighbor))
    {
      return false;
    }

    if (neighbor.HasLiquid && neighbor.Liquid != source.Liquid)
    {
      return true;
    }

    if (neighborY > sourceY && (!neighbor.HasLiquid || neighbor.LiquidAmount < byte.MaxValue))
    {
      return true;
    }

    return neighborY == sourceY && neighbor.LiquidAmount != source.LiquidAmount;
  }

  /// <summary>
  /// Reads the liquid geometry that overlaps an entity hitbox for one spatial query.
  /// </summary>
  /// <remarks>
  /// Pure read: no queue updates, no revision changes, no mutation journal. The caller supplies a
  /// capability that decides which liquid kinds the entity can perceive; anything outside it is
  /// masked out of the returned flags.
  /// </remarks>
  internal static SpatialLiquidContact GetContact(
      LiquidTileStorage storage,
      int tileSize,
      Vector2 position,
      Vector2 colliderSize,
      LiquidInteractionCapabilityComponent capability)
  {
    if (!capability.Enabled || capability.AffectedLiquids == LiquidContactFlags.None ||
        !Terraria.Physics.SimulationVector.IsFinite(position) || !Terraria.Physics.SimulationVector.IsFinite(colliderSize) ||
        colliderSize.X <= 0f || colliderSize.Y <= 0f)
    {
      return new SpatialLiquidContact(LiquidContactFlags.None, 0);
    }

    double worldWidth = (double)storage.Width * tileSize;
    double worldHeight = (double)storage.Height * tileSize;
    if ((double)position.X + colliderSize.X <= 0d ||
        (double)position.Y + colliderSize.Y <= 0d ||
        position.X >= worldWidth || position.Y >= worldHeight)
    {
      return new SpatialLiquidContact(LiquidContactFlags.None, 0);
    }

    int minX = (int)Math.Clamp(Math.Floor((double)position.X / tileSize), 0, storage.Width - 1);
    int minY = (int)Math.Clamp(Math.Floor((double)position.Y / tileSize), 0, storage.Height - 1);
    int maxX = (int)Math.Clamp(
        Math.Floor(((double)position.X + colliderSize.X - 0.0001d) / tileSize),
        0,
        storage.Width - 1);
    int maxY = (int)Math.Clamp(
        Math.Floor(((double)position.Y + colliderSize.Y - 0.0001d) / tileSize),
        0,
        storage.Height - 1);

    LiquidContactFlags flags = LiquidContactFlags.None;
    byte amount = 0;
    for (int y = minY; y <= maxY; y++)
    {
      for (int x = minX; x <= maxX; x++)
      {
        WorldTileState tile = storage.Read(new WorldTileCoordinate(x, y));
        if (!tile.HasLiquid)
        {
          continue;
        }

        float tileTop = y * tileSize;
        float liquidTop = tileTop + tileSize - tileSize * tile.LiquidAmount / byte.MaxValue;
        bool overlapsLiquid = position.Y < tileTop + tileSize &&
            position.Y + colliderSize.Y > liquidTop &&
            position.X < (x + 1) * tileSize &&
            position.X + colliderSize.X > x * tileSize;
        if (!overlapsLiquid)
        {
          continue;
        }

        LiquidContactFlags accepted = tile.Liquid & capability.AffectedLiquids;
        flags |= accepted;
        if (accepted != LiquidContactFlags.None)
        {
          amount = Math.Max(amount, tile.LiquidAmount);
        }
      }
    }

    return new SpatialLiquidContact(flags, amount);
  }

}

/// <summary>Result of one bounded liquid pass.</summary>
internal readonly record struct LiquidStepOutcome(
    int Processed,
    int Operations,
    bool Changed);

/// <summary>One tile the liquid pass rewrote, with enough state for the caller to journal it.</summary>
/// <remarks>
/// 液面步进的内部产物，只有坐标与前后状态——它是"这一步改写了什么"，不是"要投影给客户端
/// 什么"。调用方（<c>WorldRuntime</c>）负责把它投影成 <c>Terraria.World.WorldMutation</c>，
/// 那一步才补上掉落信息。
///
/// 刻意不直接复用 <c>WorldMutation</c>：那会让液面逻辑依赖世界投影模型，并被迫携带
/// 它不产生也不关心的掉落字段。
/// </remarks>
internal readonly record struct LiquidTileChange(
    WorldTileCoordinate Coordinate,
    WorldTileState Previous,
    WorldTileState Current);
