using EntityEcs;
using Terraria.Network;
using Terraria.Relationships;
using Terraria.NonAuthoritative.Persistence;

namespace NSSLC.NetworkVerification;

internal static class PlayerNetworkOwnerVerification
{
  public static async Task RunAsync()
  {
    var current = new Dictionary<ConnectionIdentity, SenderBinding>();
    await using var owner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await owner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId runtimeId = await owner.InvokeAsync(
      session => session.EntityRuntime.RuntimeId);
    var playerOwner = new NetworkPlayerOwner(
      owner,
      context => current.TryGetValue(context.Connection, out SenderBinding? binding) &&
        binding == context.Actor);

    ConnectionIdentity firstConnection = new(Guid.NewGuid(), 1);
    var firstBinding = new SenderBinding(3, Guid.NewGuid());
    current[firstConnection] = firstBinding;
    NetworkSessionContext firstContext = CreateContext(
      firstConnection,
      firstBinding,
      runtimeId,
      NetworkSessionStage.Active);

    NetworkPlayerBindingResult created = await playerOwner.EnsurePlayerAsync(firstContext);
    Verify.That(created.Status == NetworkPlayerBindingStatus.Applied &&
      created.Player is NetworkPlayerSnapshot first,
      "The owner must create the authenticated player in the shared session runtime.");
    EntityReference firstReference = created.Player!.Value.Reference;

    NetworkPlayerBindingResult existing = await playerOwner.EnsurePlayerAsync(firstContext);
    Verify.That(existing.Status == NetworkPlayerBindingStatus.Existing &&
      existing.Player!.Value.Reference == firstReference,
      "Repeated admission for the same binding must reuse the same player entity.");

    NetworkPlayerReferenceResult resolved =
      await playerOwner.ResolveAuthenticatedPlayerAsync(firstContext);
    Verify.That(resolved.Succeeded && resolved.Reference == firstReference,
      "Items must be able to resolve the authenticated player reference.");

    NetworkPlayerSnapshot? social = await playerOwner.CapturePlayerAsync(firstContext);
    Verify.That(social is { } socialSnapshot && socialSnapshot.PlayerSlot == 3 &&
      socialSnapshot.Width == 20 && socialSnapshot.Height == 42 && socialSnapshot.Active &&
      !socialSnapshot.Dead && socialSnapshot.WorldRuntimeId == runtimeId,
      "Social must receive a detached same-session player snapshot.");

    NetworkPlayerMovementResult moved = await playerOwner.CommitMovementAsync(
      firstContext,
      new NetworkPlayerMovementInput(
        ApplyPosition: true,
        new System.Numerics.Vector2(128, 256),
        ApplyVelocity: true,
        new System.Numerics.Vector2(1, -2),
        SelectedInventorySlot: 4));
    Verify.That(moved.Succeeded && moved.Player!.Value.Position == new System.Numerics.Vector2(128, 256) &&
      moved.Player.Value.SelectedInventorySlot == 4,
      "Movement and selected-slot commits must update the formal player components.");

    (bool Captured, bool Resolved) sameCallback = await owner.InvokeAsync(session => {
      bool captured = playerOwner.TryCaptureOnOwnerThread(session, firstContext, out _);
      bool resolved = playerOwner.TryResolveOnOwnerThread(session, firstContext, out _);
      return (captured, resolved);
    });
    Verify.That(sameCallback.Captured && sameCallback.Resolved,
      "Items and Social must have a synchronous same-owner callback seam.");

    NetworkPlayerBindingStatus disconnected = await playerOwner.DisconnectAsync(firstContext);
    Verify.That(disconnected == NetworkPlayerBindingStatus.Disconnected,
      "Disconnect must remove the entity and binding projection.");
    Verify.That(!await owner.InvokeAsync(session => session.EntityRuntime.TryResolve(
        firstReference,
        out _)),
      "A disconnected player's scoped reference must stop resolving.");
    // The authority has released the old connection before its slot is reused. This
    // makes the later rejection represent a stale sender binding, rather than a
    // deliberately inconsistent current-sender predicate.
    current.Remove(firstConnection);

    ConnectionIdentity secondConnection = new(Guid.NewGuid(), 2);
    var secondBinding = new SenderBinding(3, Guid.NewGuid());
    current[secondConnection] = secondBinding;
    NetworkSessionContext secondContext = CreateContext(
      secondConnection,
      secondBinding,
      runtimeId,
      NetworkSessionStage.Active);
    NetworkPlayerBindingResult rebound = await playerOwner.EnsurePlayerAsync(secondContext);
    Verify.That(rebound.Succeeded && rebound.Player!.Value.Reference != firstReference,
      "A reused slot must receive a new entity identity.");
    Verify.That((await playerOwner.ResolveAuthenticatedPlayerAsync(firstContext)).Status ==
      NetworkPlayerBindingStatus.RejectedSenderBinding,
      "The old connection binding must not resolve after slot reuse.");

    ConnectionIdentity closingConnection = new(Guid.NewGuid(), 4);
    var closingBinding = new SenderBinding(4, Guid.NewGuid());
    current[closingConnection] = closingBinding;
    NetworkSessionContext closingContext = CreateContext(
      closingConnection,
      closingBinding,
      runtimeId,
      NetworkSessionStage.Active);
    NetworkPlayerBindingResult closingPlayer =
      await playerOwner.EnsurePlayerAsync(closingContext);
    current.Remove(closingConnection);
    NetworkSessionContext capturedClosingContext = new(
      closingConnection,
      closingContext.ProfileKey,
      NetworkSessionStage.Closed,
      closingBinding,
      IsHost: false,
      WorldRuntimeId: null);
    Verify.That(await playerOwner.DisconnectAsync(capturedClosingContext) ==
      NetworkPlayerBindingStatus.Disconnected &&
      !await owner.InvokeAsync(session => session.EntityRuntime.TryResolve(
        closingPlayer.Player!.Value.Reference,
        out _)),
      "Disconnect must clean a matching captured closing context after sender release.");
  }

  private static NetworkSessionContext CreateContext(
    ConnectionIdentity connection,
    SenderBinding binding,
    EntityRuntimeId runtimeId,
    NetworkSessionStage stage)
  {
    return new NetworkSessionContext(
      connection,
      "verification",
      stage,
      binding,
      IsHost: false,
      WorldRuntimeId: runtimeId);
  }
}


