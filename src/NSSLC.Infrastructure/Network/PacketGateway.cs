using System.Net;
using System.Threading.Channels;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketGateway : IAsyncDisposable {
  private readonly object _gate = new();
  private readonly ProtocolProfile _profile;
  private readonly INetworkSessionAuthority _authority;
  private readonly bool _requiresPassword;
  private readonly TimeProvider _time;
  private readonly Func<EntityRuntimeId?>? _worldRuntimeIdProvider;
  private readonly Func<NetworkSessionContext, CancellationToken, ValueTask>? _sessionClosing;
  private readonly PacketSnapshotCache? _cache;
  private readonly Dictionary<(byte, ushort?, byte?), PacketHandlerRegistration> _handlers = new();
  private readonly Dictionary<ConnectionIdentity, NetworkSession> _sessions = new();
  private readonly Dictionary<ConnectionIdentity, TaskCompletionSource> _completion = new();
  private readonly CancellationTokenSource _stopping = new();
  private readonly SemaphoreSlim _ownerSlots;
  private readonly Channel<PacketGatewayDiagnostic> _diagnostics =
      Channel.CreateBounded<PacketGatewayDiagnostic>(new BoundedChannelOptions(1024) {
        FullMode = BoundedChannelFullMode.DropOldest
      });
  private bool _frozen;
  private bool _disposed;

  public PacketGatewayOptions Options { get; }
  public IReadOnlyList<NetworkSession> Sessions {
    get { lock (_gate) { return Array.AsReadOnly(_sessions.Values.ToArray()); } }
  }

  /// <summary>Returns a detached snapshot of explicitly registered ingress handlers.</summary>
  public IReadOnlyList<PacketRegistrationSnapshot> Registrations {
    get {
      lock (_gate) {
        return Array.AsReadOnly(_handlers.Values
            .OrderBy(registration => registration.Policy.MessageId)
            .ThenBy(registration => registration.Policy.ModuleId)
            .ThenBy(registration => registration.Policy.Action)
            .Select(registration => new PacketRegistrationSnapshot(registration.Policy,
                registration.HandlerType)).ToArray());
      }
    }
  }

  public PacketGateway(ProtocolProfile profile, INetworkSessionAuthority authority,
      PacketGatewayOptions? options = null, TimeProvider? timeProvider = null,
      PacketSnapshotCache? snapshotCache = null,
      Func<EntityRuntimeId?>? worldRuntimeIdProvider = null,
      Func<NetworkSessionContext, CancellationToken, ValueTask>? sessionClosing = null) {
    _profile = profile;
    _authority = authority;
    _requiresPassword = authority.RequiresPassword;
    Options = options ?? new();
    Options.Validate();
    _ownerSlots = new(Options.MaximumSessions, Options.MaximumSessions);
    _time = timeProvider ?? TimeProvider.System;
    _worldRuntimeIdProvider = worldRuntimeIdProvider;
    _sessionClosing = sessionClosing;
    if (snapshotCache is not null && snapshotCache.ProfileKey != profile.Key) {
      throw new ArgumentException("Snapshot cache must use the gateway's profile.");
    }
    _cache = snapshotCache;
    RequireFormat<HelloPacket>(1, PacketDirection.ClientToServer);
    RequireFormat<PlayerInfoPacket>(3, PacketDirection.ServerToClient);
    if (_requiresPassword) {
      RequireFormat<SendPasswordPacket>(38, PacketDirection.ClientToServer);
      RequireFormat<RequestPasswordPacket>(37, PacketDirection.ServerToClient);
    }
    if (Options.EnableHostAuthorization) {
      RequireFormat<HostTokenPacket>(161, PacketDirection.ClientToServer);
      RequireFormat<SetCountsAsHostForGameplayPacket>(139, PacketDirection.ServerToClient);
    }
    if (Options.EnablePing) {
      RequireFormat<NetModulesPacket>(82, PacketDirection.ClientToServer);
      RequireFormat<NetModulesPacket>(82, PacketDirection.ServerToClient);
      RequireFormat<PingPacket>(154, PacketDirection.ClientToServer);
      RequireFormat<PingPacket>(154, PacketDirection.ServerToClient);
    }
  }

  internal EntityRuntimeId? CaptureWorldRuntimeId() {
    return _worldRuntimeIdProvider?.Invoke();
  }

  /// <summary>Rechecks a captured sender before an owner commits queued work.</summary>
  public bool IsCurrentSender(NetworkSessionContext context) {
    ArgumentNullException.ThrowIfNull(context);
    NetworkSession? session;
    lock (_gate) {
      if (_disposed || !_sessions.TryGetValue(context.Connection, out session)) {
        return false;
      }
    }
    return session.Matches(context)
        && context.WorldRuntimeId == CaptureWorldRuntimeId();
  }

  public void Register<TPacket>(PacketPolicy policy, IPacketHandler<TPacket> handler) {
    ArgumentNullException.ThrowIfNull(handler);
    if (policy.MessageId is 0 or 1 or 10 or 38 or 85 or 93 or 94 or 161
        || policy.MessageId > 161
        || (policy.MessageId == 154 && Options.EnablePing)
        || policy.MaximumPerWindow < 1 || policy.MaximumBytesPerWindow < 1
        || policy.AllowedStages == 0
        || (policy.AllowedStages & ~(NetworkSessionStage.AwaitPlayerData
            | NetworkSessionStage.AwaitSectionRequest | NetworkSessionStage.Synchronizing
            | NetworkSessionStage.Active)) != 0) {
      throw new ArgumentException("Unsupported or unsafe packet policy.", nameof(policy));
    }
    if (policy.MessageId == 82) {
      if (policy.ModuleId is null or > 14 or 1 or 2 or 6 or 11 or 14
          || (policy.ModuleId == 12 && policy.Action is not (0 or 1))
          || (policy.ModuleId is 4 or 7 or 10 && policy.Action is > 2)
          || (policy.ModuleId == 9 && policy.Action != 0)
          || (policy.ModuleId == 13 && policy.Action is not (1 or 2))
          || ((policy.ModuleId is 4 or 7 or 9 or 10 or 12 or 13)
              != policy.Action.HasValue)) {
        throw new ArgumentException("Module/action needs explicit supported admission.");
      }
    } else if (policy.ModuleId is not null || policy.Action is not null) {
      throw new ArgumentException("Module selection is only valid for packet 82.");
    }
    RequireFormat<TPacket>(policy.MessageId, PacketDirection.ClientToServer);
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_frozen) {
        throw new InvalidOperationException("Gateway policy registration is frozen.");
      }
      _handlers.Add((policy.MessageId, policy.ModuleId, policy.Action), new(policy,
          (context, packet, token) => handler.HandleAsync(context, (TPacket)packet, token),
          handler.GetType().FullName ?? handler.GetType().Name));
    }
  }

  /// <summary>Publishes a detached server-owned world effect without an incoming packet.</summary>
  /// <remarks>
  /// The runtime token belongs to the committed world operation. Each target's current context
  /// is used only as its transport admission lease; no player is the event's initiating sender.
  /// A true result means local dispatch completed, not that clients acknowledged the effect.
  /// </remarks>
  public async ValueTask<bool> PublishWorldAsync(EntityRuntimeId expectedWorldRuntimeId,
      OutboundDispatch dispatch, CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(dispatch);
    cancellationToken.ThrowIfCancellationRequested();
    if (dispatch.Kind is not (PacketDispatchKind.AllActive
        or PacketDispatchKind.SectionSubscribers)
        || dispatch.AllowedStages != NetworkSessionStage.Active
        || dispatch.WorldKey != expectedWorldRuntimeId.Value) {
      throw new ArgumentException("World effects require active world-scoped routing.",
          nameof(dispatch));
    }
    _profile.Find(PacketDirection.ServerToClient, dispatch.Packet.GetType());
    NetworkSession[] candidates;
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (!expectedWorldRuntimeId.IsAssigned
          || CaptureWorldRuntimeId() != expectedWorldRuntimeId) {
        return false;
      }
      candidates = _sessions.Values
          .Where(session => session.Stage == NetworkSessionStage.Active).ToArray();
    }
    using var linked = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken, _stopping.Token);
    var deliveries = new List<Task>(candidates.Length);
    foreach (NetworkSession target in candidates) {
      NetworkSessionContext lease = target.Context();
      if (lease.WorldRuntimeId == expectedWorldRuntimeId && IsCurrentSender(lease)
          && target.CanReceive(lease, dispatch)) {
        deliveries.Add(DeliverAsync(lease, target, dispatch, default, linked.Token));
      }
    }
    await Task.WhenAll(deliveries).ConfigureAwait(false);
    linked.Token.ThrowIfCancellationRequested();
    return CaptureWorldRuntimeId() == expectedWorldRuntimeId;
  }

  public PacketTcpServer CreateServer(IPAddress address, int port,
      PacketConnectionOptions? connectionOptions = null, PacketByteBudget? budget = null) {
    Seal();
    return new(address, port, _profile, HandleAsync, connectionOptions, budget);
  }

  internal void Seal() {
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      _frozen = true;
    }
  }

  public Task HandleAsync(PacketConnection connection, CancellationToken cancellationToken = default) {
    NetworkSession session;
    var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    lock (_gate) {
      ObjectDisposedException.ThrowIf(_disposed, this);
      _frozen = true;
      if (connection.ProfileKey != _profile.Key || _sessions.Count >= Options.MaximumSessions
          || _sessions.ContainsKey(connection.Identity)) {
        throw new PacketProtocolException("ConnectionAdmissionRejected");
      }
      session = new(this, connection, _time);
      _sessions.Add(connection.Identity, session);
      _completion.Add(connection.Identity, completion);
    }
    return RunAsync(session, completion, cancellationToken);
  }

  public bool TryReadDiagnostic(out PacketGatewayDiagnostic? diagnostic) {
    return _diagnostics.Reader.TryRead(out diagnostic);
  }

  internal void Record(PacketGatewayDiagnostic diagnostic) {
    _diagnostics.Writer.TryWrite(diagnostic);
  }

  internal PacketPolicy? FindPolicy(byte id, ushort? module, byte? action,
      out PacketHandlerRegistration? handler) {
    handler = null;
    if (id == 82 && module is ushort wireModule) {
      module = ResolveModuleId(wireModule);
    }
    if (id == 1) {
      return new(id, NetworkSessionStage.AwaitHello, MaximumPerWindow: 1);
    }
    if (id == 38 && _requiresPassword) {
      return new(id, NetworkSessionStage.AwaitPassword, MaximumPerWindow: 1);
    }
    if (id == 161 && Options.EnableHostAuthorization) {
      return new(id, NetworkSessionStage.AwaitPlayerData, MaximumPerWindow: 1);
    }
    if (id == 82 && module == 2 && Options.EnablePing) {
      return new(id, NetworkSessionStage.Active, module, MaximumPerWindow: 5);
    }
    if (id == 154 && Options.EnablePing) {
      return new(id, NetworkSessionStage.Active,
          MaximumPerWindow: 5, MaximumBytesPerWindow: 1);
    }
    if (_handlers.TryGetValue((id, module, action), out handler)) {
      return handler.Policy;
    }
    return null;
  }

  internal ushort ResolveModuleId(ushort wireModule) {
    return Options.UseSteamModuleIds
        ? Packet82KnownModuleCodecsV4.ResolveLegacySteamModuleId(wireModule) : wireModule;
  }

  public async ValueTask DisposeAsync() {
    Task[] completions;
    NetworkSession[] sessions;
    lock (_gate) {
      if (_disposed) {
        return;
      }
      _disposed = true;
      sessions = _sessions.Values.ToArray();
      completions = _completion.Values.Select(item => item.Task).ToArray();
    }
    _stopping.Cancel();
    foreach (NetworkSession session in sessions) {
      await session.Connection.DisposeAsync().ConfigureAwait(false);
    }
    await Task.WhenAll(completions).ConfigureAwait(false);
    _diagnostics.Writer.TryComplete();
  }

  private async Task RunAsync(NetworkSession session, TaskCompletionSource completion,
      CancellationToken cancellationToken) {
    using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken, _stopping.Token);
    try {
      while (!lifetime.IsCancellationRequested) {
        TimeSpan remaining = session.Remaining();
        if (remaining <= TimeSpan.Zero) {
          throw new PacketProtocolException("SessionDeadlineExceeded");
        }
        using var deadline = new CancellationTokenSource(remaining, _time);
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(
            lifetime.Token, deadline.Token);
        PacketMessage? message = await session.Connection.ReadPacketAsync(operation.Token)
            .ConfigureAwait(false);
        if (message is null) {
          break;
        }
        if (message.Connection != session.Identity
            || message.Direction != PacketDirection.ClientToServer) {
          throw new PacketProtocolException("InvalidConnectionEnvelope", message.MessageId);
        }
        using var ownerDeadline = new CancellationTokenSource(Options.OwnerTimeout, _time);
        using var ownerOperation = CancellationTokenSource.CreateLinkedTokenSource(
            operation.Token, ownerDeadline.Token);
        Task handling = HandleMessageAsync(session, message, ownerOperation.Token);
        try {
          await handling.WaitAsync(ownerOperation.Token).ConfigureAwait(false);
        } catch (OperationCanceledException) when (ownerOperation.IsCancellationRequested) {
          _ = ObserveLateMessageAsync(session.Identity, handling);
          throw;
        }
      }
    } catch (OperationCanceledException) {
      Record(new(session.Identity, lifetime.IsCancellationRequested ? "Stopped" : "SessionTimeout"));
    } catch (PacketProtocolException error) {
      Record(new(session.Identity, error.Code, error.MessageId, BodyOffset: error.BodyOffset));
      if (error.Code is "VersionRejected" or "AuthenticationRejected" or "HostAuthorizationRejected") {
        await RejectAsync(session).ConfigureAwait(false);
      }
    } catch (Exception error) {
      Record(new(session.Identity, "SessionFailure:" + error.GetType().Name));
    } finally {
      await PublishPlayerDisconnectedAsync(session).ConfigureAwait(false);
      session.MoveTo(NetworkSessionStage.Closing);
      try {
        await session.Connection.DisposeAsync().ConfigureAwait(false);
        if (session.Binding is SenderBinding binding) {
          using var cleanup = new CancellationTokenSource(Options.CleanupTimeout, _time);
          if (_sessionClosing is not null) {
            await _sessionClosing.Invoke(session.Context(), cleanup.Token).AsTask()
                .WaitAsync(cleanup.Token).ConfigureAwait(false);
          }
          await _authority.ReleaseAsync(session.Identity, binding, cleanup.Token).AsTask()
              .WaitAsync(cleanup.Token).ConfigureAwait(false);
        }
      } catch (Exception error) {
        Record(new(session.Identity, "CleanupFailure:" + error.GetType().Name));
      } finally {
        session.MoveTo(NetworkSessionStage.Closed);
        lock (_gate) {
          _sessions.Remove(session.Identity);
          _completion.Remove(session.Identity);
        }
        completion.TrySetResult();
      }
    }
  }

  private async Task PublishPlayerDisconnectedAsync(NetworkSession session) {
    if (session.Stage != NetworkSessionStage.Active || session.Binding is not SenderBinding binding
        || !_profile.Bindings.Any(packetBinding =>
            packetBinding.Direction == PacketDirection.ServerToClient
            && packetBinding.PacketType == typeof(PlayerActivePacket))) {
      return;
    }

    using var cleanup = new CancellationTokenSource(Options.CleanupTimeout, _time);
    try {
      await DispatchAsync(session.Context(), new OutboundDispatch(
          new PlayerActivePacket { Player = binding.PlayerSlot, ActiveState = 0 },
          PacketDispatchKind.AllActiveExceptSender), cleanup.Token).ConfigureAwait(false);
    } catch (Exception error) {
      Record(new(session.Identity, "PlayerDisconnectProjectionFailure:" +
          error.GetType().Name, 14));
    }
  }

  private async Task HandleMessageAsync(NetworkSession session, PacketMessage message,
      CancellationToken token) {
    if (message.MessageId == 1) {
      if (!Options.IgnoreClientVersion
          && message.Get<HelloPacket>().Version != _profile.HelloVersion) {
        throw new PacketProtocolException("VersionRejected", 1);
      }
      if (_requiresPassword) {
        session.MoveTo(NetworkSessionStage.AwaitPassword);
        await session.Connection.WritePacketAsync(new RequestPasswordPacket(), token).ConfigureAwait(false);
      } else {
        await AdmitAsync(session, null, token).ConfigureAwait(false);
      }
      return;
    }
    if (message.MessageId == 38) {
      await AdmitAsync(session, message.Get<SendPasswordPacket>().Password, token).ConfigureAwait(false);
      return;
    }
    NetworkSessionContext context = session.Context();
    if (message.MessageId == 161) {
      bool approved = await InvokeOwnerAsync(() => _authority.AuthorizeHostAsync(context,
          message.Get<HostTokenPacket>().HostToken, token), token).WaitAsync(token)
          .ConfigureAwait(false);
      session.SetHost(approved);
      if (!approved) {
        Record(new(session.Identity, "HostAuthorizationDenied", 161));
        return;
      }
      await session.Connection.WritePacketAsync(new SetCountsAsHostForGameplayPacket {
        Player = context.Actor.PlayerSlot, CountsAsHost = true
      }, token).ConfigureAwait(false);
      return;
    }
    if (message.MessageId == 154 && session.Selected is null) {
      _ = message.Get<PingPacket>();
      await session.Connection.WritePacketAsync(new PingPacket(), token).ConfigureAwait(false);
      return;
    }
    if (message.MessageId == 82 && session.Selected is null) {
      NetModulesPacket ping = message.Get<NetModulesPacket>();
      if (ping.ModuleId != 2 || ping.Data is not Packet82PingData data) {
        throw new PacketProtocolException("InvalidPing", 82);
      }
      await session.Connection.WritePacketAsync(new NetModulesPacket {
        ModuleId = 2, Data = data
      }, token).ConfigureAwait(false);
      return;
    }
    PacketHandlerRegistration registration = session.Selected
        ?? throw new PacketProtocolException("HandlerMissing", message.MessageId);
    PacketHandlingResult result = await InvokeOwnerAsync(
        () => registration.Handle.Invoke(context, message.Payload, token), token).WaitAsync(token)
        .ConfigureAwait(false);
    token.ThrowIfCancellationRequested();
    if (!result.Accepted || result.Outbound.Count > Options.MaximumDispatchesPerMessage) {
      throw new PacketProtocolException(result.RejectionCode ?? "OwnerRejected", message.MessageId);
    }
    session.Apply(message.MessageId, result);
    foreach (OutboundDispatch dispatch in result.Outbound) {
      await DispatchAsync(session.Context(), dispatch, token).ConfigureAwait(false);
    }
  }

  private async Task AdmitAsync(NetworkSession session, string? password, CancellationToken token) {
    Task<SessionAdmission> pending = InvokeOwnerAsync(
        () => _authority.AdmitAsync(session.Identity, password, token), token);
    SessionAdmission admission;
    try {
      admission = await pending.WaitAsync(token).ConfigureAwait(false);
    } catch (OperationCanceledException) when (token.IsCancellationRequested) {
      _ = ReleaseLateAdmissionAsync(session.Identity, pending);
      throw;
    }
    if (admission.Binding is null) {
      throw new PacketProtocolException(admission.RejectionCode ?? "AuthenticationRejected");
    }
    bool bound;
    lock (_gate) {
      bound = session.Bind(admission.Binding);
      if (bound && _sessions.Values.Any(other => other.Identity != session.Identity
          && other.Binding == admission.Binding)) {
        throw new PacketProtocolException("DuplicateSenderBinding");
      }
    }
    if (!bound) {
      using var cleanup = new CancellationTokenSource(Options.CleanupTimeout, _time);
      await _authority.ReleaseAsync(session.Identity, admission.Binding, cleanup.Token).AsTask()
          .WaitAsync(cleanup.Token).ConfigureAwait(false);
      throw new PacketProtocolException("SessionClosed");
    }
    token.ThrowIfCancellationRequested();
    await session.Connection.WritePacketAsync(new PlayerInfoPacket {
      Player = admission.Binding.PlayerSlot, Accepted = false
    }, token).ConfigureAwait(false);
  }

  private async Task DispatchAsync(NetworkSessionContext sender, OutboundDispatch dispatch,
      CancellationToken token) {
    NetworkSession[] targets;
    lock (_gate) {
      if (!IsSenderCurrent(sender)) {
        Record(new(sender.Connection, "SenderExpired"));
        return;
      }
      targets = _sessions.Values.Where(target => target.CanReceive(sender, dispatch)).ToArray();
    }
    PacketBinding binding = _profile.Find(PacketDirection.ServerToClient, dispatch.Packet.GetType());
    ReadOnlyMemory<byte> cached = default;
    if (_cache is not null && binding.MessageId == 10 && dispatch.SnapshotRevision.HasValue) {
      var key = new PacketSnapshotCacheKey(_profile.Key, dispatch.WorldKey,
          dispatch.WorldGeneration, dispatch.Section, dispatch.SnapshotRevision.Value,
          dispatch.SnapshotVariant);
      if (!_cache.TryGet(key, out cached)) {
        await _cache.TryStoreObjectAsync(key, dispatch.Packet, token).ConfigureAwait(false);
        _cache.TryGet(key, out cached);
      }
    }
    await Task.WhenAll(targets.Select(target => DeliverAsync(sender, target, dispatch, cached, token)))
        .ConfigureAwait(false);
  }

  private async Task DeliverAsync(NetworkSessionContext sender, NetworkSession target,
      OutboundDispatch dispatch, ReadOnlyMemory<byte> cached, CancellationToken token) {
    if (!target.CanReceive(sender, dispatch)) {
      Record(new(target.Identity, "TargetExpired"));
      return;
    }
    try {
      Func<bool> admission = () => target.CanReceive(sender, dispatch) && IsSenderCurrent(sender);
      Task<PacketWriteReceipt> pending = cached.IsEmpty
          ? target.Connection.WriteObjectAsync(dispatch.Packet, token, admission).AsTask()
          : target.Connection.WriteSnapshotFrameAsync(cached, token, admission).AsTask();
      PacketWriteReceipt receipt;
      try {
        receipt = await pending.WaitAsync(token).ConfigureAwait(false);
      } catch (OperationCanceledException) when (token.IsCancellationRequested && !pending.IsCompleted) {
        await target.Connection.DisposeAsync().ConfigureAwait(false);
        receipt = await pending.ConfigureAwait(false);
      }
      Record(new(target.Identity, "LocallySent", receipt.MessageId));
    } catch (PacketSendException error) {
      Record(new(target.Identity, error.Code, SendCertainty: error.Certainty));
      await target.Connection.DisposeAsync().ConfigureAwait(false);
    } catch (OperationCanceledException) when (token.IsCancellationRequested) {
      Record(new(target.Identity, "DispatchCanceled", SendCertainty: PacketSendCertainty.NotSubmitted));
    }
  }

  private void RequireFormat<TPacket>(byte id, PacketDirection direction) {
    if (_profile.Find(direction, id).PacketType != typeof(TPacket)) {
      throw new ArgumentException($"Packet {id}/{direction} has an incompatible payload type.");
    }
  }

  private async Task RejectAsync(NetworkSession session) {
    session.MoveTo(NetworkSessionStage.Closing);
    try {
      using var deadline = new CancellationTokenSource(Options.CleanupTimeout, _time);
      await session.Connection.WritePacketAsync(new KickPacket {
        Text = NetworkText.Literal("Connection rejected.")
      }, deadline.Token).AsTask().WaitAsync(deadline.Token).ConfigureAwait(false);
    } catch (Exception error) {
      Record(new(session.Identity, "RejectionSendFailure:" + error.GetType().Name));
    }
  }

  private bool IsSenderCurrent(NetworkSessionContext context) {
    return IsCurrentSender(context);
  }

  private async Task ObserveLateMessageAsync(ConnectionIdentity identity, Task pending) {
    try {
      await pending.ConfigureAwait(false);
    } catch (OperationCanceledException) {
      Record(new(identity, "LateMessageCanceled"));
    } catch (Exception error) {
      Record(new(identity, "LateMessageFailure:" + error.GetType().Name));
    }
  }

  private async Task<TResult> InvokeOwnerAsync<TResult>(Func<ValueTask<TResult>> operation,
      CancellationToken token) {
    await _ownerSlots.WaitAsync(token).ConfigureAwait(false);
    try {
      return await operation.Invoke().ConfigureAwait(false);
    } finally {
      _ownerSlots.Release();
    }
  }

  private async Task ReleaseLateAdmissionAsync(ConnectionIdentity identity,
      Task<SessionAdmission> pending) {
    try {
      SessionAdmission admission = await pending.ConfigureAwait(false);
      if (admission.Binding is not null) {
        using var cleanup = new CancellationTokenSource(Options.CleanupTimeout, _time);
        await _authority.ReleaseAsync(identity, admission.Binding, cleanup.Token).AsTask()
            .WaitAsync(cleanup.Token).ConfigureAwait(false);
      }
    } catch (OperationCanceledException) {
      Record(new(identity, "LateAdmissionCanceled"));
    } catch (Exception error) {
      Record(new(identity, "LateAdmissionFailure:" + error.GetType().Name));
    }
  }
}
