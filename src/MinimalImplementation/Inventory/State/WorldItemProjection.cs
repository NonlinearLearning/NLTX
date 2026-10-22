using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.State;

public enum WorldItemProjectionKind
{
  Item,
  Ownership,
  Inventory,
  Player,
  Shimmer,
  OwnershipReleaseRequest
}

/// <summary>A committed, target-specific value. Empty Item is a retirement tombstone.</summary>
public sealed record WorldItemProjection(
  long Sequence,
  SessionEpoch Target,
  WorldScopeId WorldScope,
  WorldItemProjectionKind Kind,
  WorldItemHandle Handle,
  CommitVersion Revision,
  WorldItemSnapshot? Item = null,
  WorldItemReservation? Reservation = null,
  InventorySnapshot? Inventory = null,
  Vector2 Position = default,
  SessionPacketProjection? PlayerProjection = null,
  SessionEpoch? InventoryOwnerEpoch = null);
