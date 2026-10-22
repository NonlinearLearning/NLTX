namespace MinimalImplementation.Modules.Creative;

/// <summary>Describes local transport settlement for one creative-power effect batch.</summary>
public enum CreativePowerEffectDeliveryStatus
{
  NoRecipients,
  Queued,
  LocallySent,
  NotSubmitted,
  OutcomeUnknown,
  Failed,
  InvalidReceipt,
  StaleVersion
}

/// <summary>
/// Describes local transport settlement only. A settled result is not a client acknowledgement;
/// a valid report retains successes and leaves failed, unknown or missing intents pending.
/// Every reported attempt ends; pending intents require CapturePendingDelivery with fresh tokens.
/// An invalid report accepts no new successes. Stale attempts cannot settle a later capture.
/// </summary>
public sealed record CreativePowerEffectDeliveryResult(
  CreativePowerEffectDeliveryStatus Status,
  string? Detail)
{
  public bool IsSettled => Status is CreativePowerEffectDeliveryStatus.NoRecipients
    or CreativePowerEffectDeliveryStatus.Queued
    or CreativePowerEffectDeliveryStatus.LocallySent;

  public static CreativePowerEffectDeliveryResult Invalid(string detail) => new(
    CreativePowerEffectDeliveryStatus.InvalidReceipt,
    detail);

  public static CreativePowerEffectDeliveryResult Stale(string detail) => new(
    CreativePowerEffectDeliveryStatus.StaleVersion,
    detail);

  public static CreativePowerEffectDeliveryResult NoRecipients() => new(
    CreativePowerEffectDeliveryStatus.NoRecipients,
    "The creative-power effect has no outbound recipients.");
}
