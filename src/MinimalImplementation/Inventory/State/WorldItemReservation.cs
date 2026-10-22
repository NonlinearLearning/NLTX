using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record WorldItemReservation(
  WorldItemHandle Handle,
  EntityUuid PlayerId,
  SessionEpoch Epoch,
  DateTimeOffset ReservedAt,
  DateTimeOffset ExpiresAt,
  CommitVersion Revision,
  byte? WirePlayerSlot = null,
  bool FrameBased = false,
  int ReleaseCounter = 0);
