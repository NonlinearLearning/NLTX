using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Result returned by the P04 loadout transaction port and the P02 projection.
/// P02 stores only the selected loadout and visibility projection, never inventory contents.
/// </summary>
public sealed record PlayerLoadoutCommit(
  OwnerCommitResult Result,
  byte LoadoutIndex,
  ushort AccessoryVisibilityMask)
{
  public IReadOnlyList<SessionPacketProjection> Projections { get; init; } =
    Array.Empty<SessionPacketProjection>();
}
