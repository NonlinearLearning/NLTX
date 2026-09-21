using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class GrassSpreadRecursionPolicy
{
  public const int MaximumDepth = 1000;

  public static bool ShouldRecurse(GrassSpreadRecursionState state, bool repeat)
  {
    Validate(state);
    return repeat && state.Depth < MaximumDepth;
  }

  public static GrassSpreadRecursionState Enter(GrassSpreadRecursionState state)
  {
    Validate(state);
    if (state.Depth >= MaximumDepth)
    {
      throw new InvalidOperationException("Grass spread recursion reached its maximum depth.");
    }

    return new GrassSpreadRecursionState(state.Depth + 1);
  }

  public static GrassSpreadRecursionState Exit(GrassSpreadRecursionState state)
  {
    Validate(state);
    if (state.Depth == 0)
    {
      throw new InvalidOperationException("Grass spread recursion cannot exit an empty state.");
    }

    return new GrassSpreadRecursionState(state.Depth - 1);
  }

  private static void Validate(GrassSpreadRecursionState state)
  {
    if (state.Depth < 0 || state.Depth > MaximumDepth)
    {
      throw new ArgumentOutOfRangeException(nameof(state));
    }
  }
}
