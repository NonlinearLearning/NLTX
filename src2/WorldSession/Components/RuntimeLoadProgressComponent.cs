namespace Terraria.WorldSession.Components;

public sealed class RuntimeLoadProgressComponent
{
  public int TotalWorkUnits { get; internal set; }

  public int CompletedWorkUnits { get; internal set; }

  public bool IsComplete { get; internal set; }

  public string? LastStatusText { get; internal set; }
}
