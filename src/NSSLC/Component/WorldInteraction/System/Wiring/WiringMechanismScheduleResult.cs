namespace Terraria.WorldInteraction.Wiring;

public readonly record struct WiringMechanismScheduleResult(
  bool Succeeded,
  bool Changed,
  string? FailureReason)
{
  public static WiringMechanismScheduleResult Accepted =>
    new(true, true, null);

  public static WiringMechanismScheduleResult Rejected(string reason) =>
    new(false, false, reason);
}
