using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

/// <summary>Assigns vanilla item simulation to the nearest active client on the owner loop.</summary>
public sealed class VanillaWorldItemOwnership {
  private const float MaximumOwnerDistance = 1920f;
  private readonly ServerComposition _composition;

  public VanillaWorldItemOwnership(ServerComposition composition) {
    _composition = composition ?? throw new ArgumentNullException(nameof(composition));
  }

  public void SetTargets() {
    WorldScopeId scope = new(_composition.IdentityRegistry.CurrentWorldScope);
    if (scope.IsValid) {
      _composition.WorldItemStore.Projections.SetTargets(scope,
        _composition.SessionAdmission.CaptureActiveSessionEpochs());
    }
  }

  public OwnerCommitResult Advance() {
    WorldScopeId scope = new(_composition.IdentityRegistry.CurrentWorldScope);
    if (!scope.IsValid) {
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    }
    SetTargets();
    var players = new List<(PlayerEntity Player, WorldItemOwnerBindingCapture Binding)>();
    foreach (SessionEpoch epoch in _composition.SessionAdmission.CaptureActiveSessionEpochs()) {
      if (_composition.Players.TryGetBoundPlayer(epoch, _composition.SessionAdmission,
          out var player) && player is not null && !player.IsDead &&
          WorldItemOwnerBindingCapture.TryCapture(_composition.SessionAdmission,
            player.Handle.Id, epoch, scope, out var binding) && binding is not null) {
        players.Add((player, binding));
      }
    }
    WorldItemReservation[] claims = _composition.WorldItemReservationStore.CaptureWorld(scope)
      .ToArray();
    foreach (WorldItemSnapshot item in _composition.WorldItemStore.CaptureWorld(scope)) {
      WorldItemReservation? current = claims.FirstOrDefault(value => value.Handle == item.Handle);
      if (current is not null && _composition.SessionAdmission.TryGetStage(
          current.Epoch, out SessionStage ownerStage) && ownerStage == SessionStage.Active) {
        continue;
      }
      WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
      if (runtime.NoGrabDelay > 0 && runtime.OwnIgnore == -1 || item.ShimmerTime > 0) {
        continue;
      }
      var nearest = players.Where(candidate => runtime.OwnTime <= 0 ||
          runtime.OwnIgnore != candidate.Binding.Binding.PlayerSlot)
        .OrderBy(candidate => Distance(item, candidate.Player)).FirstOrDefault();
      if (nearest.Player is null || Distance(item, nearest.Player) > MaximumOwnerDistance) {
        continue;
      }
      WorldItemOwnerBindingCapture owner = nearest.Binding;
      ReservationResult result = _composition.WorldItemReservationStore.TryReserveFrame(item, owner);
      if (result.Status is not (OwnerCommitStatus.Applied or OwnerCommitStatus.Conflict)) {
        return new OwnerCommitResult(result.Status, null, result.ErrorCode, result.Detail);
      }
      if (result.Status == OwnerCommitStatus.Applied) {
        ServerLog.Info("server.item.owner_assigned",
          $"epoch={owner.Binding.Epoch.Value} slot={item.Handle.SlotId} type={item.Item.Type} " +
          $"player={owner.Binding.PlayerSlot} grabDelay={runtime.OwnTime}");
      }
    }
    return OwnerCommitResult.Applied(CommitVersion.Initial);
  }

  private static float Distance(WorldItemSnapshot item, PlayerEntity player) {
    float x = player.Movement.Position.X + player.RuntimeSpatial.Width / 2f;
    float y = player.Movement.Position.Y + player.RuntimeSpatial.Height / 2f;
    return MathF.Abs(x - item.Position.X - 8) + MathF.Abs(y - item.Position.Y - 8);
  }
}
