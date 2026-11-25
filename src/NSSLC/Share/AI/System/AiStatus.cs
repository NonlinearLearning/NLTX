namespace Terraria.Share.Ai;

/// <summary>
/// Calculation status, never an acknowledgement of authoritative action execution.
/// </summary>
public enum AiStatus {
  Calculated,
  Disabled,
  MissingComponents,
  BindingChanged,
  Stopped
}
