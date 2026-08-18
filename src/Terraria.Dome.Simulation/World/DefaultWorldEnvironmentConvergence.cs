using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class DefaultWorldEnvironmentConvergence
{
  private const int DefaultWorldHeight = 1200;
  private const int DefaultWorldWidth = 4200;
  private const int TileSquareConvergenceDivisor = 3;
  private const byte LiquidChangeType = 0;
  private const byte TileManipulationKillAction = 0;
  private const ushort AmbientTileType = 2;
  private const byte LiquidAmount = byte.MaxValue;
  private static readonly TileSquareSchedule[] TileSquareSchedules =
  [
    new(5, 1636, 301, 1, 1), new(60, 2195, 412, 3, 3),
    new(61, 2194, 413, 3, 3), new(74, 2289, 293, 1, 1),
    new(126, 1783, 249, 1, 1), new(250, 1976, 293, 1, 1),
    new(260, 1837, 510, 3, 3), new(261, 1836, 511, 3, 3),
    new(264, 1980, 293, 1, 1), new(272, 2189, 533, 3, 3),
    new(273, 2188, 532, 3, 3), new(289, 1784, 259, 1, 1),
    new(305, 2368, 315, 1, 1), new(315, 1877, 305, 1, 1),
    new(360, 2177, 311, 1, 1), new(534, 1626, 522, 3, 3),
    new(535, 1627, 523, 3, 3), new(568, 1757, 251, 1, 1),
    new(573, 2171, 336, 1, 1), new(660, 1790, 256, 1, 1),
    new(720, 1769, 255, 1, 1), new(733, 2216, 294, 1, 1),
    new(753, 2378, 316, 1, 1)
  ];

  private readonly List<WorldEnvironmentChange> _pendingChanges;
  private int _nextChangeIndex;
  private long _tick;

  private DefaultWorldEnvironmentConvergence(List<WorldEnvironmentChange> pendingChanges)
  {
    _pendingChanges = pendingChanges;
  }

  public bool IsComplete => _nextChangeIndex == _pendingChanges.Count;

  public static DefaultWorldEnvironmentConvergence Create(WorldGrid world)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (world.Width != DefaultWorldWidth || world.Height != DefaultWorldHeight)
    {
      return new DefaultWorldEnvironmentConvergence([]);
    }

    List<WorldEnvironmentChange> changes = new(
      TileSquareSchedules.Length + 1 + 106);
    WorldTile ambientTile = new(IsActive: true, AmbientTileType);
    for (int index = 0; index < TileSquareSchedules.Length; index++)
    {
      TileSquareSchedule schedule = TileSquareSchedules[index];
      changes.Add(new WorldEnvironmentChange(
        schedule.DueTick / TileSquareConvergenceDivisor,
        WorldEnvironmentChangeKind.TileSquare,
        schedule.X,
        schedule.Y,
        schedule.Width,
        schedule.Height,
        LiquidChangeType,
        ambientTile));
    }

    changes.Add(new WorldEnvironmentChange(
      620 / TileSquareConvergenceDivisor,
      WorldEnvironmentChangeKind.TileManipulation,
      3179,
      830,
      1,
      1,
      TileManipulationKillAction,
      default));

    for (int index = 0; index < 106; index++)
    {
      long dueTick = 5 + index * 2L;
      bool isVisibleLiquid = index == 37 || index == 39;
      int x = isVisibleLiquid ? 1837 : 100;
      int y = isVisibleLiquid ? 510 : 400;
      byte amount = isVisibleLiquid ? LiquidAmount : (byte)((index & 1) + 1);
      changes.Add(new WorldEnvironmentChange(
        dueTick,
        WorldEnvironmentChangeKind.Liquid,
        x,
        y,
        1,
        1,
        LiquidChangeType,
        new WorldTile(IsActive: true, AmbientTileType, amount, LiquidChangeType)));
    }

    changes.Sort(WorldEnvironmentChangeComparer.Instance);
    return new DefaultWorldEnvironmentConvergence(changes);
  }

  public IReadOnlyList<WorldEnvironmentChange> Advance(WorldGrid world)
  {
    ArgumentNullException.ThrowIfNull(world);
    _tick++;
    List<WorldEnvironmentChange> committed = new();
    while (_nextChangeIndex < _pendingChanges.Count &&
           _pendingChanges[_nextChangeIndex].DueTick <= _tick)
    {
      WorldEnvironmentChange change = _pendingChanges[_nextChangeIndex];
      Apply(world, change);
      committed.Add(change);
      _nextChangeIndex++;
    }

    return committed;
  }

  private static void Apply(WorldGrid world, WorldEnvironmentChange change)
  {
    if (change.Kind == WorldEnvironmentChangeKind.Liquid)
    {
      _ = world.TrySetLiquid(change.X, change.Y, change.Tile.LiquidAmount, change.Tile.LiquidType);
      return;
    }

    for (int yOffset = 0; yOffset < change.Height; yOffset++)
    {
      for (int xOffset = 0; xOffset < change.Width; xOffset++)
      {
        _ = world.TrySetTile(change.X + xOffset, change.Y + yOffset, change.Tile);
      }
    }
  }

  private readonly record struct TileSquareSchedule(
    long DueTick,
    int X,
    int Y,
    byte Width,
    byte Height);

  private sealed class WorldEnvironmentChangeComparer : IComparer<WorldEnvironmentChange>
  {
    public static readonly WorldEnvironmentChangeComparer Instance = new();

    public int Compare(WorldEnvironmentChange first, WorldEnvironmentChange second)
    {
      int dueTickComparison = first.DueTick.CompareTo(second.DueTick);
      if (dueTickComparison != 0)
      {
        return dueTickComparison;
      }

      return first.Kind.CompareTo(second.Kind);
    }
  }
}
