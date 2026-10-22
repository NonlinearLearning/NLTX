using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ReservationResult(
  OwnerCommitStatus Status,
  WorldItemReservation? Reservation,
  ProtocolErrorCode? ErrorCode,
  string? Detail)
{
  public static ReservationResult Applied(WorldItemReservation reservation) =>
    new(OwnerCommitStatus.Applied, reservation, null, null);

  public static ReservationResult Stale(string detail) =>
    new(OwnerCommitStatus.Stale, null, ProtocolErrorCode.StaleEntity, detail);

  public static ReservationResult Conflict(string detail) =>
    new(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  public static ReservationResult Rejected(ProtocolErrorCode code, string detail) =>
    new(OwnerCommitStatus.Rejected, null, code, detail);

  public static ReservationResult Unavailable(string detail) =>
    new(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);
}
