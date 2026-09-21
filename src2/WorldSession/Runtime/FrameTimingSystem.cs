using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public sealed class FrameTimingSystem
{
  public void Update(
    FrameTimingAndSchedulingStateComponent state,
    FrameTimingInput input)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (double.IsNaN(input.TotalSeconds) || double.IsInfinity(input.TotalSeconds) ||
      input.TotalSeconds < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input));
    }

    state.WrappedHour = (float)(input.TotalSeconds % 3600d);
    state.GlobalTimerPaused = input.GlobalTimerPaused;
  }
}
