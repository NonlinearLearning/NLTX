using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Network;

/// <summary>
/// Applies authenticated packet-27 and packet-29 commands through the active world's projectile
/// lifecycle owner. It owns no world or runtime references between commands.
/// </summary>
public sealed class ProjectileNetworkCommandOwner : IProjectileNetworkCommandOwner
{
  private const int ServerProjectileOwnerSlot = byte.MaxValue;

  private readonly IProjectileNetworkCommandSessionQuery _sessions;
  private readonly IProjectileNetworkOwnerThreadScheduler _scheduler;

  public ProjectileNetworkCommandOwner(
    IProjectileNetworkCommandSessionQuery sessions,
    IProjectileNetworkOwnerThreadScheduler scheduler)
  {
    _sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
    _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
  }

  public ValueTask<PacketHandlingResult> ApplyProjectileAsync(
    NetworkSessionContext sender,
    ProjectileNetworkApplyCommand command,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(sender);
    cancellationToken.ThrowIfCancellationRequested();

    return _scheduler.ExecuteAsync(
      () => ApplyOnOwnerThread(sender, command, cancellationToken),
      cancellationToken);
  }

  public ValueTask<PacketHandlingResult> TerminateProjectileAsync(
    NetworkSessionContext sender,
    ProjectileNetworkTerminateCommand command,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(sender);
    cancellationToken.ThrowIfCancellationRequested();

    return _scheduler.ExecuteAsync(
      () => TerminateOnOwnerThread(sender, command, cancellationToken),
      cancellationToken);
  }

  private PacketHandlingResult ApplyOnOwnerThread(
    NetworkSessionContext sender,
    ProjectileNetworkApplyCommand command,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (!TryResolveCurrentSession(sender, out ProjectileNetworkCommandSession? session))
    {
      return Reject("StaleProjectileSession");
    }

    int expectedOwnerSlot = command.ProjectileType == 949
      ? ServerProjectileOwnerSlot
      : sender.Actor.PlayerSlot;
    if (command.OwnerSlot != expectedOwnerSlot)
    {
      return Reject("ProjectileOwnerMismatch");
    }

    if (!session!.ProjectileDefinitions.TryGet(
      command.ProjectileType,
      out Terraria.Content.ProjectileDefinition definition) ||
      definition is null)
    {
      return AcceptWithoutDispatch();
    }

    if (command.ProjectileType != 949 && definition.Combat.Hostile)
    {
      return AcceptWithoutDispatch();
    }

    cancellationToken.ThrowIfCancellationRequested();
    var lifecycle = new ProjectileLifecycleSystem(session.WorldSession.Storage);
    if (!lifecycle.TryApplyNetwork(
      command,
      session.ProjectileDefinitions,
      session.HydrationContext,
      out ProjectileHandle handle))
    {
      // Source packet handling silently ignores unsupported or rejected projectile state.
      return AcceptWithoutDispatch();
    }

    if (!lifecycle.TryGetRuntimeHandle(handle, out RuntimeEntityHandle runtimeHandle) ||
      runtimeHandle.RuntimeId != session.WorldSession.EntityRuntime.RuntimeId)
    {
      throw new InvalidOperationException(
        "The applied projectile handle does not belong to the active world runtime.");
    }

    // Projectile packet dispatch remains disabled at the host boundary. The lifecycle commit is
    // accepted here without producing a relay dispatch.
    return AcceptWithoutDispatch();
  }

  private PacketHandlingResult TerminateOnOwnerThread(
    NetworkSessionContext sender,
    ProjectileNetworkTerminateCommand command,
    CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (!TryResolveCurrentSession(sender, out ProjectileNetworkCommandSession? session))
    {
      return Reject("StaleProjectileSession");
    }

    if (command.OwnerSlot != sender.Actor.PlayerSlot)
    {
      return Reject("ProjectileOwnerMismatch");
    }

    cancellationToken.ThrowIfCancellationRequested();
    var lifecycle = new ProjectileLifecycleSystem(session!.WorldSession.Storage);
    // Missing, stale, inactive, or repeated packet-29 requests retain the source accepted-no-op
    // behavior and do not dispatch a relay.
    lifecycle.TryTerminateNetwork(command);
    return AcceptWithoutDispatch();
  }

  private bool TryResolveCurrentSession(
    NetworkSessionContext sender,
    out ProjectileNetworkCommandSession? session)
  {
    if (sender.Stage != NetworkSessionStage.Active ||
      sender.Actor.PlayerSlot == ServerProjectileOwnerSlot ||
      sender.Actor.GameSessionKey == Guid.Empty ||
      !sender.WorldRuntimeId.HasValue ||
      !_sessions.TryResolveCurrent(sender, out session) ||
      session is null ||
      session.Sender != sender ||
      session.WorldSession.IsDisposed ||
      !session.WorldSession.IsComplete ||
      !session.WorldSession.IsPublished ||
      session.WorldSession.IsPublicationUncertain ||
      session.WorldSession.EntityRuntime.RuntimeId != sender.WorldRuntimeId.Value ||
      !ReferenceEquals(session.WorldSession.EntityRuntime, session.WorldSession.Storage.EntityRuntime))
    {
      session = null;
      return false;
    }

    return true;
  }

  private static PacketHandlingResult AcceptWithoutDispatch()
  {
    return new PacketHandlingResult(accepted: true);
  }

  private static PacketHandlingResult Reject(string rejectionCode)
  {
    return new PacketHandlingResult(accepted: false, rejectionCode: rejectionCode);
  }
}
