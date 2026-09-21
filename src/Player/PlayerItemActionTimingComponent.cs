namespace Terraria.Player;

// Stores the paired Player item-action timers and the tool-use marker.
// Item definitions, reuse/input policy, Wiring and Combat effects remain external.
public sealed class PlayerItemActionTimingComponent
{
  public int AnimationRemaining { get; set; }

  public int AnimationDuration { get; set; }

  public int UseRemaining { get; set; }

  public int UseDuration { get; set; }

  public int ToolUseMarker { get; set; }
}
