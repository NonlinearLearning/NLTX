using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>
/// Detached pending-transfer membership from the authoritative ordered emergency pass.
/// Validate guards its revision/incarnation and item generations without processing transfers.
/// Missing captures are NotReady; an invented empty set is not an authoritative capture.
/// </summary>
public interface IWorldItemEmergencyStackingCapture : IWorldItemTransactionParticipant
{
  WorldScopeId WorldScope { get; }
  bool HasPendingTransfer(WorldItemHandle handle);
}
