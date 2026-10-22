using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.Protocol;

/// <summary>Resolves a player slot only from the current authenticated Active binding.</summary>
public sealed class ActiveSessionPlayerSlotResolver
{
  private readonly SessionAdmissionCoordinator _sessions;

  public ActiveSessionPlayerSlotResolver(SessionAdmissionCoordinator sessions)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    _sessions = sessions;
  }

  public bool TryResolve(
    EntityUuid playerId, SessionEpoch epoch, out byte playerSlot)
  {
    playerSlot = default;
    if (!playerId.IsValid || !epoch.IsValid
      || !_sessions.TryGetStage(epoch, out SessionStage stage)
      || stage != SessionStage.Active
      || !_sessions.TryGetBinding(epoch, out SessionBinding? binding)
      || binding is null
      || binding.PlayerHandle.Id != playerId
      || !binding.PlayerHandle.WorldScope.IsValid
      || !binding.PlayerHandle.Generation.IsValid)
    {
      return false;
    }

    playerSlot = binding.PlayerSlot;
    return true;
  }

  public byte? Resolve(EntityUuid playerId, SessionEpoch epoch)
  {
    return TryResolve(playerId, epoch, out byte playerSlot) ? playerSlot : null;
  }

  public bool TryCapture(
    EntityUuid playerId, SessionEpoch epoch, WorldScopeId scope,
    out WorldItemOwnerBindingCapture? capture) =>
    WorldItemOwnerBindingCapture.TryCapture(_sessions, playerId, epoch, scope, out capture);
}
