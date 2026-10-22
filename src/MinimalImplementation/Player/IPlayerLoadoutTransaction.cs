using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Player;

/// <summary>A prepared P04 inventory operation consumed by the P02 loadout coordinator.</summary>
/// <remarks>
/// Preparation and Validate must not write state. After every participant validates, Commit
/// executes synchronously in the same server-loop window, without I/O or business rejection.
/// P04 must check inventory revisions and perform the actual equipment swaps exactly once.
/// </remarks>
public interface IPlayerLoadoutTransaction
{
  OwnerCommitResult Validate();

  void Commit();
}
