// Adapted from CrashKonijn/GOAP IActionConfig; see LICENSE.txt and SOURCES.md.
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime;

/// <summary>Configure before graph construction; treat arrays and properties as immutable.</summary>
public sealed class ActionConfig {
  public string Name { get; init; } = string.Empty;
  public float BaseCost { get; init; } = 1f;
  public float StoppingDistance { get; init; } = 1f;
  public bool ValidateTarget { get; init; } = true;
  public bool RequiresTarget { get; init; } = true;
  public bool ValidateConditions { get; init; } = true;
  public ActionMoveMode MoveMode { get; init; } = ActionMoveMode.MoveBeforePerforming;
  public ICondition[] Conditions { get; init; } = [];
  public IEffect[] Effects { get; init; } = [];
  public IActionProperties Properties { get; init; } = new EmptyActionProperties();
}
