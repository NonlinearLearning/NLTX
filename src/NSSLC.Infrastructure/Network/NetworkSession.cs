using System.Buffers.Binary;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class NetworkSession {
  private sealed class RateCounter {
    public long Start;
    public int Count;
    public int Bytes;
  }

  private readonly object _gate = new();
  private readonly PacketGateway _gateway;
  private readonly Dictionary<(byte, ushort?, byte?), RateCounter> _rates = new();
  private readonly TimeProvider _time;
  private readonly long _started;
  private long _stageStarted;
  private long _lastActivity;
  private NetworkSessionStage _stage = NetworkSessionStage.AwaitHello;
  private SenderBinding? _binding;
  private bool _isHost;
  private SectionInterestProjection? _interest;
  private PacketHandlerRegistration? _selected;

  public ConnectionIdentity Identity => Connection.Identity;
  public NetworkSessionStage Stage {
    get { lock (_gate) { return _stage; } }
  }
  public SenderBinding? Binding {
    get { lock (_gate) { return _binding; } }
  }
  public SectionInterestProjection? Interest {
    get { lock (_gate) { return _interest; } }
  }
  internal PacketConnection Connection { get; }
  internal PacketHandlerRegistration? Selected => _selected;

  internal NetworkSession(PacketGateway gateway, PacketConnection connection, TimeProvider time) {
    _gateway = gateway;
    Connection = connection;
    _time = time;
    _started = _stageStarted = _lastActivity = time.GetTimestamp();
    connection.Admission = Admit;
  }

  internal bool Admit(byte id, ReadOnlyMemory<byte> body) {
    NetworkSessionStage stage = Stage;
    _selected = null;
    if (stage is NetworkSessionStage.Closing or NetworkSessionStage.Closed) {
      return false;
    }
    ushort? module = null;
    byte? action = null;
    if (id == 82) {
      if (body.Length < 2) {
        return false;
      }
      module = BinaryPrimitives.ReadUInt16LittleEndian(body.Span);
      if (module is 4 or 7 or 9 or 10 or 12 or 13) {
        int offset = module == 12 ? 3 : 2;
        if (body.Length <= offset) {
          return false;
        }
        action = body.Span[offset];
      }
    }
    PacketPolicy? policy = _gateway.FindPolicy(id, module, action, out _selected);
    if (policy is null || (policy.AllowedStages & stage) == 0
        || (policy.RequiresHost && !Context().IsHost)) {
      _gateway.Record(new(Identity, "AdmissionRejected", id));
      return false;
    }
    var key = (id, module, action);
    long now = _time.GetTimestamp();
    if (!_rates.TryGetValue(key, out RateCounter? rate)) {
      rate = new() { Start = now };
      _rates.Add(key, rate);
    }
    if (_time.GetElapsedTime(rate.Start, now) >= _gateway.Options.RateWindow) {
      rate.Start = now;
      rate.Count = rate.Bytes = 0;
    }
    if (rate.Count >= policy.MaximumPerWindow
        || body.Length > policy.MaximumBytesPerWindow - rate.Bytes) {
      _gateway.Record(new(Identity, "RateLimitExceeded", id));
      return false;
    }
    rate.Count++;
    rate.Bytes += body.Length;
    _lastActivity = now;
    return true;
  }

  internal NetworkSessionContext Context() {
    lock (_gate) {
      return new(Identity, Connection.ProfileKey, _stage,
          _binding ?? throw new PacketProtocolException("SenderNotBound"), _isHost);
    }
  }

  internal TimeSpan Remaining() {
    lock (_gate) {
      PacketGatewayOptions options = _gateway.Options;
      if (_stage == NetworkSessionStage.Active) {
        return options.ActiveIdleTimeout - _time.GetElapsedTime(_lastActivity);
      }
      if (_stage == NetworkSessionStage.Synchronizing) {
        return options.SynchronizationTimeout - _time.GetElapsedTime(_stageStarted);
      }
      TimeSpan stage = options.StageTimeout - _time.GetElapsedTime(_stageStarted);
      TimeSpan total = options.HandshakeTimeout - _time.GetElapsedTime(_started);
      return stage < total ? stage : total;
    }
  }

  internal bool Bind(SenderBinding binding) {
    if (binding.PlayerSlot == byte.MaxValue || binding.GameSessionKey == Guid.Empty) {
      throw new PacketProtocolException("InvalidSenderBinding");
    }
    lock (_gate) {
      if (_stage is NetworkSessionStage.Closing or NetworkSessionStage.Closed) {
        return false;
      }
      _binding = binding;
      MoveTo(NetworkSessionStage.AwaitPlayerData);
    }
    return true;
  }

  internal void SetHost() {
    lock (_gate) {
      if (_stage is NetworkSessionStage.Closing or NetworkSessionStage.Closed) {
        throw new PacketProtocolException("SessionClosed");
      }
      _isHost = true;
    }
  }

  internal void MoveTo(NetworkSessionStage next) {
    lock (_gate) {
      if (_stage == NetworkSessionStage.Closed) {
        return;
      }
      if (_stage == NetworkSessionStage.Closing) {
        if (next == NetworkSessionStage.Closing) {
          return;
        }
        if (next != NetworkSessionStage.Closed) {
          throw new PacketProtocolException("SessionClosed");
        }
      }
      _stage = next;
      _stageStarted = _time.GetTimestamp();
    }
    _gateway.Record(new(Identity, "Stage:" + next));
  }

  internal void Apply(byte id, PacketHandlingResult result) {
    lock (_gate) {
      if (_stage is NetworkSessionStage.Closing or NetworkSessionStage.Closed) {
        throw new PacketProtocolException("SessionClosed", id);
      }
      ApplyCore(id, result);
    }
  }

  private void ApplyCore(byte id, PacketHandlingResult result) {
    NetworkSessionStage current = _stage;
    if (result.NextStage is NetworkSessionStage next) {
      bool valid = (current, id, next) is
          (NetworkSessionStage.AwaitPlayerData, 6, NetworkSessionStage.AwaitSectionRequest)
          or (NetworkSessionStage.AwaitSectionRequest, 8, NetworkSessionStage.Synchronizing)
          or (NetworkSessionStage.Synchronizing, 12, NetworkSessionStage.Active);
      if (!valid) {
        throw new PacketProtocolException("InvalidStageTransition", id);
      }
      MoveTo(next);
    }
    if (result.Interest is not null) {
      lock (_gate) {
        if (_interest is not null && result.Interest.WorldKey == _interest.WorldKey
            && (result.Interest.WorldGeneration < _interest.WorldGeneration
                || (result.Interest.WorldGeneration == _interest.WorldGeneration
                    && result.Interest.Revision < _interest.Revision))) {
          throw new PacketProtocolException("StaleInterestProjection", id);
        }
        _interest = result.Interest;
      }
    }
  }

  internal bool CanReceive(NetworkSessionContext sender, OutboundDispatch dispatch) {
    lock (_gate) {
      if ((_stage is NetworkSessionStage.Closing or NetworkSessionStage.Closed)
          || _binding is null || _binding.GameSessionKey != sender.Actor.GameSessionKey
          || Connection.ProfileKey != sender.ProfileKey
          || (dispatch.AllowedStages & _stage) == 0) {
        return false;
      }
      if (dispatch.Kind == PacketDispatchKind.AllActiveExceptSender) {
        return _stage == NetworkSessionStage.Active && Identity != sender.Connection;
      }
      if (dispatch.Kind == PacketDispatchKind.SectionSubscribers) {
        return _stage == NetworkSessionStage.Active && _interest is not null
            && _interest.WorldKey == dispatch.WorldKey
            && _interest.WorldGeneration == dispatch.WorldGeneration
            && _interest.Sections.Contains(dispatch.Section);
      }
      return dispatch.Targets.Contains(Identity);
    }
  }
}
