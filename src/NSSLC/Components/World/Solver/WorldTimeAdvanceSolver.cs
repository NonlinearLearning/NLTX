using System;

namespace Terraria.World;

/// <summary>
/// Pure validation and derivation rules for the world clock and revision counters.
/// </summary>
/// <remarks>
/// This type computes only; <see cref="WorldRuntimeStateComponent" /> owns the resulting state
/// and performs the assignment. Rules and failure messages live here so the state holder stays a
/// data owner.
/// </remarks>
public static class WorldTimeAdvanceSolver
{
  /// <summary>Validates one logical tick and derives the next clock values.</summary>
  public static bool TryAdvance(
      long previousTick,
      long revision,
      double totalTimeSeconds,
      double timeOfDaySeconds,
      double dayLengthSeconds,
      long tick,
      float deltaTime,
      out WorldTimeAdvance advance,
      out string? failure)
  {
    advance = default;
    failure = null;
    if (tick < 0 ||
        (previousTick < 0
            ? tick != 0
            : previousTick == long.MaxValue || tick != previousTick + 1))
    {
      failure = "The world tick must advance by exactly one logical step.";
      return false;
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      failure = "The world delta time must be finite and positive.";
      return false;
    }

    if (revision == long.MaxValue)
    {
      failure = "The world revision is exhausted.";
      return false;
    }

    double nextTotal = totalTimeSeconds + deltaTime;
    if (!double.IsFinite(nextTotal))
    {
      failure = "The world time is exhausted.";
      return false;
    }

    double nextTimeOfDay = timeOfDaySeconds + deltaTime;
    nextTimeOfDay %= dayLengthSeconds;
    if (nextTimeOfDay < 0d)
    {
      nextTimeOfDay += dayLengthSeconds;
    }

    advance = new WorldTimeAdvance(
        tick,
        nextTotal,
        nextTimeOfDay,
        nextTimeOfDay < dayLengthSeconds / 2d,
        revision + 1);
    return true;
  }

  /// <summary>True when the revision counter can still be advanced.</summary>
  public static bool CanAdvanceRevision(long revision)
  {
    return revision != long.MaxValue;
  }

  /// <summary>Derives the next revision value or reports exhaustion.</summary>
  public static bool TryIncrementRevision(
      long revision,
      out long nextRevision,
      out string? failure)
  {
    failure = null;
    if (revision == long.MaxValue)
    {
      nextRevision = revision;
      failure = "The world revision is exhausted.";
      return false;
    }

    nextRevision = revision + 1;
    return true;
  }

  /// <summary>Derives the epoch and revision values for a world reload.</summary>
  public static bool TryBeginReload(
      ulong worldEpoch,
      long revision,
      out ulong nextEpoch,
      out long nextRevision,
      out string? failure)
  {
    nextEpoch = worldEpoch;
    nextRevision = revision;
    failure = null;
    if (worldEpoch == ulong.MaxValue)
    {
      failure = "The world epoch is exhausted.";
      return false;
    }

    if (revision == long.MaxValue)
    {
      failure = "The world revision is exhausted.";
      return false;
    }

    nextEpoch = worldEpoch + 1;
    nextRevision = revision + 1;
    return true;
  }
}
