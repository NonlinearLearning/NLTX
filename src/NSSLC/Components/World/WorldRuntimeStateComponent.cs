using System;
using Terraria.Physics;

namespace Terraria.World;

/// <summary>Stores the authoritative metadata for one in-memory world instance.</summary>
/// <remarks>
/// The component deliberately contains no tile collection. Dense tile values belong to
/// <see cref="WorldTileStorageComponent" /> so metadata updates and tile access have separate
/// owners. A server tick is accepted only when it follows the previous tick exactly.
/// </remarks>
public sealed class WorldRuntimeStateComponent
{
  public const int DefaultTileSize = 16;
  public const double DefaultDayLengthSeconds = 3_600d;

  public WorldRuntimeStateComponent(
      int width,
      int height,
      int tileSize = DefaultTileSize,
      ulong worldEpoch = 1,
      long revision = 0,
      double dayLengthSeconds = DefaultDayLengthSeconds)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (tileSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileSize));
    }

    if (worldEpoch == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    if (!double.IsFinite(dayLengthSeconds) || dayLengthSeconds <= 0d)
    {
      throw new ArgumentOutOfRangeException(nameof(dayLengthSeconds));
    }

    Width = width;
    Height = height;
    TileSize = tileSize;
    WorldEpoch = worldEpoch;
    Revision = revision;
    DayLengthSeconds = dayLengthSeconds;
    LastTick = -1;
    DayTime = true;
  }

  public int Width { get; }

  public int Height { get; }

  public int TileSize { get; }

  public ulong WorldEpoch { get; private set; }

  public long Revision { get; private set; }

  /// <summary>
  /// 投影到共享空间查询用的世界版本。投影只在这里定义一次，避免调用点各自重写。
  /// </summary>
  public SpatialWorldRevision SpatialRevision => new(WorldEpoch, Revision);

  public long LastTick { get; private set; }

  public double TotalTimeSeconds { get; private set; }

  public double TimeOfDaySeconds { get; private set; }

  public double DayLengthSeconds { get; }

  public bool DayTime { get; private set; }

  internal bool TryAdvance(
      long tick,
      float deltaTime,
      out string? failure)
  {
    if (!WorldTimeAdvanceSolver.TryAdvance(
        LastTick,
        Revision,
        TotalTimeSeconds,
        TimeOfDaySeconds,
        DayLengthSeconds,
        tick,
        deltaTime,
        out WorldTimeAdvance advance,
        out failure))
    {
      return false;
    }

    LastTick = advance.Tick;
    TotalTimeSeconds = advance.TotalTimeSeconds;
    TimeOfDaySeconds = advance.TimeOfDaySeconds;
    DayTime = advance.DayTime;
    Revision = advance.Revision;
    return true;
  }

  internal bool TryAdvanceRevision(out string? failure)
  {
    if (!WorldTimeAdvanceSolver.TryIncrementRevision(Revision, out long next, out failure))
    {
      return false;
    }

    Revision = next;
    return true;
  }

  internal bool TryReload(out string? failure)
  {
    if (!WorldTimeAdvanceSolver.TryBeginReload(
        WorldEpoch,
        Revision,
        out ulong nextEpoch,
        out long nextRevision,
        out failure))
    {
      return false;
    }

    WorldEpoch = nextEpoch;
    Revision = nextRevision;
    TotalTimeSeconds = 0d;
    TimeOfDaySeconds = 0d;
    DayTime = true;
    return true;
  }
}
