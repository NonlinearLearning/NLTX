using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// A server-selected nearby destination. The caller supplies candidates in the frozen server
/// ordering and resolves lock or in-use state from authoritative world and lease owners.
/// </summary>
public sealed record QuickStackDestinationCandidate
{
  public ChestSnapshot Chest { get; }
  public bool IsLockedOrInUse { get; }

  public QuickStackDestinationCandidate(ChestSnapshot chest, bool isLockedOrInUse)
  {
    ArgumentNullException.ThrowIfNull(chest);
    Chest = chest;
    IsLockedOrInUse = isLockedOrInUse;
  }
}
