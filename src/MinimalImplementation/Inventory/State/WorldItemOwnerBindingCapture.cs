using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>An authenticated binding captured and revalidated in an exclusive ServerLoop window.</summary>
public sealed class WorldItemOwnerBindingCapture : IWorldItemTransactionParticipant
{
  private readonly SessionAdmissionCoordinator _sessions;

  private WorldItemOwnerBindingCapture(
    SessionAdmissionCoordinator sessions, SessionBinding binding)
  {
    _sessions = sessions;
    Binding = binding;
  }

  public SessionBinding Binding { get; }

  public static bool TryCapture(
    SessionAdmissionCoordinator sessions, EntityUuid playerId, SessionEpoch epoch,
    WorldScopeId scope, out WorldItemOwnerBindingCapture? capture)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    capture = null;
    if (!playerId.IsValid || !epoch.IsValid || !scope.IsValid
      || !sessions.TryGetStage(epoch, out SessionStage stage) || stage != SessionStage.Active
      || !sessions.TryGetBinding(epoch, out SessionBinding? binding) || binding is null
      || binding.Epoch != epoch || binding.PlayerSlot == byte.MaxValue
      || binding.PlayerHandle.Id != playerId || binding.PlayerHandle.WorldScope != scope
      || !binding.PlayerHandle.Generation.IsValid)
    {
      return false;
    }

    capture = new WorldItemOwnerBindingCapture(sessions, binding);
    return true;
  }

  public OwnerCommitResult Validate()
  {
    if (!_sessions.TryGetStage(Binding.Epoch, out SessionStage stage)
      || stage != SessionStage.Active
      || !_sessions.TryGetBinding(Binding.Epoch, out SessionBinding? current)
      || !ReferenceEquals(current, Binding))
    {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
        ProtocolErrorCode.StaleEpoch, "The captured world-item owner binding changed.");
    }

    return OwnerCommitResult.Applied(CommitVersion.Initial);
  }

  public void Commit()
  {
    // Read participant; the reservation store owns the assignment and captured wire slot.
  }
}
