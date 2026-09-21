using System.Numerics;

namespace Terraria.Player;

public static class PlayerInstantMovementAccumulatorSystem
{
  public static PlayerInstantMovementAccumulatorComponent CreateFrameState()
  {
    return new PlayerInstantMovementAccumulatorComponent
    {
      AccumulatedMovementThisFrame = Vector2.Zero,
    };
  }

  public static void BeginFrame(
    ref PlayerInstantMovementAccumulatorComponent state)
  {
    state.AccumulatedMovementThisFrame = Vector2.Zero;
  }

  public static void Accumulate(
    in PlayerInstantMovementAccumulatorInput input,
    ref PlayerInstantMovementAccumulatorComponent state)
  {
    state.AccumulatedMovementThisFrame += input.MovementDelta;
  }
}
