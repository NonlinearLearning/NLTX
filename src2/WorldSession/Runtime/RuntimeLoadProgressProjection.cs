using Terraria.WorldSession.Components;

namespace Terraria.WorldSession.Runtime;

public readonly record struct RuntimeLoadProgressProjection(
  int TotalWorkUnits,
  int CompletedWorkUnits,
  bool IsComplete,
  string? StatusText)
{
  public static RuntimeLoadProgressProjection From(RuntimeLoadProgressComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return new RuntimeLoadProgressProjection(
      state.TotalWorkUnits,
      state.CompletedWorkUnits,
      state.IsComplete,
      state.LastStatusText);
  }
}
