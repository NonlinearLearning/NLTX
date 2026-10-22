using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestSession(
  EntityUuid PlayerId,
  SessionEpoch Epoch,
  WorldScopeId WorldScope,
  ChestId ChestId,
  ChestGeneration ChestGeneration,
  Guid LeaseToken,
  DateTimeOffset OpenedAt,
  DateTimeOffset ExpiresAt,
  PacketSequence? LastSequence);
