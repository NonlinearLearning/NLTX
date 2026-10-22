using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>Immutable P02 projection consumed by the P04 loadout transaction boundary.</summary>
public sealed record PlayerLoadoutFacts(
  SessionEpoch Epoch,
  EntityHandle PlayerHandle,
  byte PlayerSlot,
  byte LoadoutIndex,
  ushort AccessoryVisibilityMask,
  CommitVersion PlayerVersion);
