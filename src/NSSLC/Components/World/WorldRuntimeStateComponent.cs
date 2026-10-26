using System;

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
    failure = null;
    if (tick < 0 ||
        (LastTick < 0
            ? tick != 0
            : LastTick == long.MaxValue || tick != LastTick + 1))
    {
      failure = "The world tick must advance by exactly one logical step.";
      return false;
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      failure = "The world delta time must be finite and positive.";
      return false;
    }

    if (Revision == long.MaxValue)
    {
      failure = "The world revision is exhausted.";
      return false;
    }

    double nextTotal = TotalTimeSeconds + deltaTime;
    if (!double.IsFinite(nextTotal))
    {
      failure = "The world time is exhausted.";
      return false;
    }

    double nextTimeOfDay = TimeOfDaySeconds + deltaTime;
    nextTimeOfDay %= DayLengthSeconds;
    if (nextTimeOfDay < 0d)
    {
      nextTimeOfDay += DayLengthSeconds;
    }

    LastTick = tick;
    TotalTimeSeconds = nextTotal;
    TimeOfDaySeconds = nextTimeOfDay;
    DayTime = nextTimeOfDay < DayLengthSeconds / 2d;
    Revision++;
    return true;
  }

  internal bool TryAdvanceRevision(out string? failure)
  {
    failure = null;
    if (Revision == long.MaxValue)
    {
      failure = "The world revision is exhausted.";
      return false;
    }

    Revision++;
    return true;
  }

  internal bool TryReload(out string? failure)
  {
    failure = null;
    if (WorldEpoch == ulong.MaxValue)
    {
      failure = "The world epoch is exhausted.";
      return false;
    }

    if (Revision == long.MaxValue)
    {
      failure = "The world revision is exhausted.";
      return false;
    }

    WorldEpoch++;
    Revision++;
    TotalTimeSeconds = 0d;
    TimeOfDaySeconds = 0d;
    DayTime = true;
    return true;
  }
}
