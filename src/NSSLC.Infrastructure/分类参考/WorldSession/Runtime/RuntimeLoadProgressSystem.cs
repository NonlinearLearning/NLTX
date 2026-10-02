using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public sealed class RuntimeLoadProgressSystem
{
  public void Begin(RuntimeLoadProgressComponent state, int totalWorkUnits)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (totalWorkUnits < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(totalWorkUnits));
    }

    state.TotalWorkUnits = totalWorkUnits;
    state.CompletedWorkUnits = 0;
    state.IsComplete = totalWorkUnits == 0;
    state.LastStatusText = null;
  }

  public void Advance(RuntimeLoadProgressComponent state, int workUnits, string? statusText = null)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (workUnits < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(workUnits));
    }

    checked
    {
      state.CompletedWorkUnits = Math.Min(
        state.TotalWorkUnits,
        state.CompletedWorkUnits + workUnits);
    }

    state.IsComplete = state.CompletedWorkUnits >= state.TotalWorkUnits;
    if (statusText is not null)
    {
      state.LastStatusText = statusText;
    }
  }

  public void Fail(RuntimeLoadProgressComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.CompletedWorkUnits = 0;
    state.IsComplete = false;
    state.LastStatusText = null;
  }
}
