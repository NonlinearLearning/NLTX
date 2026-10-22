namespace MinimalImplementation.Modules.Creative;

public enum CreativePermissionDeliveryStatus
{
  Queued,
  LocallySent,
  NotSubmitted,
  OutcomeUnknown,
  Failed,
  InvalidReceipt,
  StaleVersion
}

/// <summary>
/// Describes local transport settlement for one Creative permission snapshot batch.
/// This is not a client-level acknowledgement.
/// </summary>
public sealed record CreativePermissionDeliveryResult(
  CreativePermissionDeliveryStatus Status,
  string? Detail)
{
  public bool IsSettled => Status is CreativePermissionDeliveryStatus.Queued
    or CreativePermissionDeliveryStatus.LocallySent;

  public static CreativePermissionDeliveryResult Invalid(string detail) => new(
    CreativePermissionDeliveryStatus.InvalidReceipt,
    detail);

  public static CreativePermissionDeliveryResult Stale(string detail) => new(
    CreativePermissionDeliveryStatus.StaleVersion,
    detail);
}
