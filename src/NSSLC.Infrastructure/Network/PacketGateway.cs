using System.Net;
using System.Threading.Channels;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketGateway : IAsyncDisposable {
  private readonly object _gate = new();
  private readonly ProtocolProfile _profile;
  private readonly INetworkSessionAuthority _authority;
  private readonly bool _requiresPassword;
  private readonly TimeProvider _time;
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

  public PacketGateway(ProtocolProfile profile, INetworkSessionAuthority authority,
      PacketGatewayOptions? options = null, TimeProvider? timeProvider = null,
      PacketSnapshotCache? snapshotCache = null) {
    _profile = profile;
    _authority = authority;
    _requiresPassword = authority.RequiresPassword;
    Options = options ?? new();
    Options.Validate();
    _ownerSlots = new(Options.MaximumSessions, Options.MaximumSessions);
    _time = timeProvider ?? TimeProvider.System;
    if (snapshotCache is not null && snapshotCache.ProfileKey != profile.Key) {
      throw new ArgumentException("Snapshot cache must use the gateway's profile.");
    }
    _cache = snapshotCache;
    RequireFormat<Packet1Packet>(1, PacketDirection.ClientToServer);
    RequireFormat<Packet3Packet>(3, PacketDirection.ServerToClient);
    if (_requiresPassword) {
      RequireFormat<Packet38Packet>(38, PacketDirection.ClientToServer);
      RequireFormat<Packet37Packet>(37, PacketDirection.ServerToClient);
    }
    if (Options.EnableHostAuthorization) {
      RequireFormat<Packet161Packet>(161, PacketDirection.ClientToServer);
      RequireFormat<Packet139Packet>(139, PacketDirection.ServerToClient);
    }
    if (Options.EnablePing) {
      RequireFormat<Packet82Packet>(82, PacketDirection.ClientToServer);
      RequireFormat<Packet82Packet>(82, PacketDirection.ServerToClient);
    }
  }

  public void Register<TPacket>(PacketPolicy policy, IPacketHandler<TPacket> handler) {
    ArgumentNullException.ThrowIfNull(handler);
    if (policy.MessageId is 0 or 1 or 10 or 15 or 25 or 26 or 38 or 44 or 67 or 83
        or 85 or 93 or 94 or 138 or 161 || policy.MessageId > 161
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
          (context, packet, token) => handler.HandleAsync(context, (TPacket)packet, token)));
    }
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
    if (_handlers.TryGetValue((id, module, action), out handler)) {
      return handler.Policy;
    }
    return null;
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
      session.MoveTo(NetworkSessionStage.Closing);
      try {
        await session.Connection.DisposeAsync().ConfigureAwait(false);
        if (session.Binding is SenderBinding binding) {
          using var cleanup = new CancellationTokenSource(Options.CleanupTimeout, _time);
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

  private async Task HandleMessageAsync(NetworkSession session, PacketMessage message,
      CancellationToken token) {
    if (message.MessageId == 1) {
      if (message.Get<Packet1Packet>().Version != _profile.HelloVersion) {
        throw new PacketProtocolException("VersionRejected", 1);
      }
      if (_requiresPassword) {
        session.MoveTo(NetworkSessionStage.AwaitPassword);
        await session.Connection.WritePacketAsync(new Packet37Packet(), token).ConfigureAwait(false);
      } else {
        await AdmitAsync(session, null, token).ConfigureAwait(false);
      }
      return;
    }
    if (message.MessageId == 38) {
      await AdmitAsync(session, message.Get<Packet38Packet>().Password, token).ConfigureAwait(false);
      return;
    }
    NetworkSessionContext context = session.Context();
    if (message.MessageId == 161) {
      bool approved = await InvokeOwnerAsync(() => _authority.AuthorizeHostAsync(context,
          message.Get<Packet161Packet>().Payload.HostToken, token), token).WaitAsync(token)
          .ConfigureAwait(false);
      if (!approved) {
        throw new PacketProtocolException("HostAuthorizationRejected", 161);
      }
      session.SetHost();
      await session.Connection.WritePacketAsync(new Packet139Packet {
        Payload = new(context.Actor.PlayerSlot, true)
      }, token).ConfigureAwait(false);
      return;
    }
    if (message.MessageId == 82 && session.Selected is null) {
      Packet82Packet ping = message.Get<Packet82Packet>();
      if (ping.Payload.ModuleId != 2 || ping.Payload.Payload is not Packet82EmptyModulePayload) {
        throw new PacketProtocolException("InvalidPing", 82);
      }
      await session.Connection.WritePacketAsync(new Packet82Packet {
        Payload = new(2, new Packet82EmptyModulePayload())
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
    await session.Connection.WritePacketAsync(new Packet3Packet {
      Payload = new(admission.Binding.PlayerSlot, false)
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
      await session.Connection.WritePacketAsync(new Packet2Packet {
        Text = NetworkText.Literal("Connection rejected.")
      }, deadline.Token).AsTask().WaitAsync(deadline.Token).ConfigureAwait(false);
    } catch (Exception error) {
      Record(new(session.Identity, "RejectionSendFailure:" + error.GetType().Name));
    }
  }

  private bool IsSenderCurrent(NetworkSessionContext context) {
    lock (_gate) {
      return _sessions.TryGetValue(context.Connection, out NetworkSession? session)
          && session.Stage is not (NetworkSessionStage.Closing or NetworkSessionStage.Closed)
          && session.Binding == context.Actor;
    }
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
