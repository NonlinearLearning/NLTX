using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Prepared ownership and counter decision, published only after the joint commit.</summary>
public sealed record WorldItemOwnershipChange(
  WorldItemSnapshot Item,
  WorldItemReservation? Before,
  WorldItemOwnerBindingCapture? Owner,
  int ReleaseCounter,
  bool PublishOwnership,
  bool RequestRelease = false,
  SessionEpoch? IgnoreEpoch = null,
  bool RetainOwner = false);
