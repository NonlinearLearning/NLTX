using EntityEcs;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;

namespace Terraria.Network;

public enum NetworkPlayerLoadoutStatus : byte
{
  Applied,
  NotFound,
  RejectedStage,
  RejectedSenderBinding,
  RejectedWorldRuntime,
  RejectedPlayerSlot,
  RejectedInvalidState,
}

public readonly record struct NetworkPlayerLoadoutResult(
  NetworkPlayerLoadoutStatus Status,
  NetworkPlayerSnapshot? Player,
  int RequestedLoadoutIndex,
  int CurrentLoadoutIndex,
  ushort AccessoryVisibilityMask,
  PlayerLoadoutSwitchRejectionReason LoadoutRejectionReason)
{
  public bool Succeeded => Status == NetworkPlayerLoadoutStatus.Applied;
}

public sealed partial class NetworkPlayerOwner
{
  /// <summary>
  /// Applies packet 147 to the authenticated Player entity. Loadout exchange and
  /// accessory visibility are committed together on the owner thread; inventory
  /// item payloads remain owned by the Items boundary.
  /// </summary>
  public ValueTask<NetworkPlayerLoadoutResult> ApplyLoadoutAsync(
    NetworkSessionContext context,
    byte targetLoadoutIndex,
    ushort accessoryVisibilityMask,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyLoadoutOnOwnerThread(
        session,
        context,
        targetLoadoutIndex,
        accessoryVisibilityMask),
      cancellationToken);
  }

  private NetworkPlayerLoadoutResult ApplyLoadoutOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    byte targetLoadoutIndex,
    ushort accessoryVisibilityMask)
  {
    NetworkPlayerBindingStatus validation = ValidateContext(
      session,
      context,
      allowAwaitPlayerData: true);
    if (validation == NetworkPlayerBindingStatus.Applied &&
      context.Stage == NetworkSessionStage.AwaitPlayerData &&
      targetLoadoutIndex < PlayerLoadoutStateComponent.LoadoutCount)
    {
      // Login may upload the loadout before packet 4 supplies the character identity.
      NetworkPlayerBindingResult binding = EnsurePlayerOnOwnerThread(session, context);
      validation = binding.Succeeded ? NetworkPlayerBindingStatus.Applied : binding.Status;
    }
    NetworkPlayerLoadoutStatus status = validation switch
    {
      NetworkPlayerBindingStatus.RejectedStage =>
        NetworkPlayerLoadoutStatus.RejectedStage,
      NetworkPlayerBindingStatus.RejectedSenderBinding =>
        NetworkPlayerLoadoutStatus.RejectedSenderBinding,
      NetworkPlayerBindingStatus.RejectedWorldRuntime =>
        NetworkPlayerLoadoutStatus.RejectedWorldRuntime,
      NetworkPlayerBindingStatus.RejectedPlayerSlot =>
        NetworkPlayerLoadoutStatus.RejectedPlayerSlot,
      _ => NetworkPlayerLoadoutStatus.NotFound,
    };
    if (validation != NetworkPlayerBindingStatus.Applied)
    {
      return RejectedLoadoutResult(status, targetLoadoutIndex);
    }

    if (targetLoadoutIndex >= PlayerLoadoutStateComponent.LoadoutCount ||
      !_playersBySlot.TryGetValue(context.Actor.PlayerSlot, out BoundPlayer? bound) ||
      bound.Connection != context.Connection ||
      bound.Binding != context.Actor)
    {
      return RejectedLoadoutResult(
        targetLoadoutIndex >= PlayerLoadoutStateComponent.LoadoutCount
          ? NetworkPlayerLoadoutStatus.RejectedInvalidState
          : NetworkPlayerLoadoutStatus.RejectedSenderBinding,
        targetLoadoutIndex);
    }

    if (!session.EntityRuntime.TryCapture(
      bound.Handle,
      static (PlayerLifecycleComponent lifecycle) => lifecycle.IsDead,
      out bool isDead))
    {
      return RejectedLoadoutResult(
        NetworkPlayerLoadoutStatus.NotFound,
        targetLoadoutIndex);
    }

    PlayerLoadoutNetworkResult? process = null;
    bool edited = session.EntityRuntime.TryEditComponents<
      PlayerLoadoutStateComponent,
      PlayerEquipmentRelationComponent,
      PlayerAppearanceSelectionComponent>(
      bound.Handle,
      (ref PlayerLoadoutStateComponent loadouts,
        ref PlayerEquipmentRelationComponent equipment,
        ref PlayerAppearanceSelectionComponent appearance) =>
      {
        var network = new PlayerLoadoutNetworkSystem(
          new PlayerLoadoutSystem(equipment, appearance, loadouts),
          new PlayerAccessoryVisibilitySystem(appearance));
        process = network.Process(
          new PlayerLoadoutNetworkRequest(
            Guid.NewGuid(),
            context.Actor.PlayerSlot,
            context.Actor.PlayerSlot,
            context.Actor.PlayerSlot,
            targetLoadoutIndex,
            UsingOrReusingItem: false,
            CCed: false,
            Dead: isDead,
            accessoryVisibilityMask));
      });
    if (!edited || process is not PlayerLoadoutNetworkResult applied)
    {
      return RejectedLoadoutResult(
        edited
          ? NetworkPlayerLoadoutStatus.RejectedInvalidState
          : NetworkPlayerLoadoutStatus.NotFound,
        targetLoadoutIndex);
    }

    NetworkPlayerSnapshot? snapshot = CaptureBoundPlayer(session, bound);
    if (snapshot is not NetworkPlayerSnapshot player ||
      !applied.Visibility.Applied)
    {
      return RejectedLoadoutResult(
        NetworkPlayerLoadoutStatus.RejectedInvalidState,
        targetLoadoutIndex);
    }

    return new NetworkPlayerLoadoutResult(
      NetworkPlayerLoadoutStatus.Applied,
      player,
      targetLoadoutIndex,
      applied.Loadout.CurrentLoadoutIndex,
      applied.Visibility.Snapshot.VisibilityMask,
      applied.Loadout.RejectionReason);
  }

  private static NetworkPlayerLoadoutResult RejectedLoadoutResult(
    NetworkPlayerLoadoutStatus status,
    int requestedLoadoutIndex)
  {
    return new NetworkPlayerLoadoutResult(
      status,
      null,
      requestedLoadoutIndex,
      requestedLoadoutIndex,
      0,
      PlayerLoadoutSwitchRejectionReason.None);
  }
}
