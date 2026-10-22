using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Session;

/// <summary>
/// Commits session stages, admission, and authenticated packet sequence on the server loop.
/// </summary>
public sealed class SessionAdmissionCoordinator : IAuthenticatedPlayerPacketGate
{
  public const byte HelloMessageId = 1;
  public const byte KickMessageId = 2;
  public const byte PlayerInfoMessageId = 3;
  public const byte PlayerActiveMessageId = 14;
  public const byte RequestPasswordMessageId = 37;
  public const byte SendPasswordMessageId = 38;
  public const byte RequestWorldDataMessageId = 6;
  public const byte WorldDataMessageId = 7;
  public const byte SpawnTileDataMessageId = 8;
  public const byte StatusTextSizeMessageId = 9;
  public const byte TileSectionMessageId = 10;
  public const byte InitialSpawnMessageId = 49;
  public const byte PlayerSpawnMessageId = 12;
  public const byte FinishedConnectingMessageId = 129;
  public const byte SetCountsAsHostForGameplayMessageId = 139;
  public const byte PlayerDeathMessageId = 118;
  public const byte DeadPlayerMessageId = 135;
  public const byte SyncProjectileTrackersMessageId = 142;
  public const byte PlayerLifeManaMessageId = 16;
  public const byte Unknown42MessageId = 42;
  public const byte SyncPlayerMessageId = 4;
  public const byte SyncEquipmentMessageId = 5;
  public const byte Unknown68MessageId = 68;
  public const byte PlayerControlsMessageId = 13;
  public const byte TogglePvpMessageId = 30;
  public const byte SyncPlayerZoneMessageId = 36;
  public const byte TeamChangeMessageId = 45;
  public const byte PlayerBuffsMessageId = 50;
  public const byte AddPlayerBuffMessageId = 55;
  public const byte DodgeMessageId = 62;
  public const byte PlayerStealthMessageId = 84;
  public const byte OpenSignRequestMessageId = 46;
  public const byte OpenSignResponseMessageId = 47;
  public const byte SyncTilePickingMessageId = 125;
  public const byte TeamChangeFromUiMessageId = 157;
  public const byte ExtraSpawnSectionLoadedMessageId = 158;
  public const byte UpdatePlayerLuckFactorsMessageId = 134;
  public const byte MinionRestTargetMessageId = 99;
  public const byte MinionAttackTargetMessageId = 115;
  public const byte PlayerHealMessageId = 35;
  public const byte Unknown66MessageId = 66;
  public const byte SpectatePlayerMessageId = 150;
  public const byte PingMessageId = 154;
  public const byte SyncLoadoutMessageId = 147;
  public const byte NetModulesMessageId = 82;

  internal CommitVersion AllocateCommitVersion()
  {
    return NextCommitVersion();
  }

  private readonly Dictionary<SessionEpoch, SessionRecord> _sessions = [];
  private readonly SessionBindingStore _bindings;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionPolicy _policy;
  private CommitVersion _commitVersion = CommitVersion.Initial;
  private bool _hasCommitted;

  public SessionAdmissionCoordinator(
    SessionBindingStore bindings,
    PlayerOwner players,
    SessionAdmissionPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(bindings);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(policy);
    _bindings = bindings;
    _players = players;
    _policy = policy;
  }

  public OwnerCommitResult OpenConnection(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEpoch, "Invalid session epoch.");
    }

    if (_sessions.ContainsKey(epoch))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "Session epoch is already open.");
    }

    _sessions.Add(epoch, new SessionRecord(epoch));
    return OwnerCommitResult.Applied(NextCommitVersion());
  }

  public SessionAdmissionSnapshot? Capture(SessionEpoch epoch)
  {
    if (!_sessions.TryGetValue(epoch, out SessionRecord? session))
    {
      return null;
    }

    return new SessionAdmissionSnapshot(
      session.Epoch,
      session.Stage,
      session.LastSequence < 0 ? null : new PacketSequence(session.LastSequence),
      session.Revision);
  }

  public bool TryGetBinding(SessionEpoch epoch, out SessionBinding? binding)
  {
    return _bindings.TryGetByEpoch(epoch, out binding);
  }

  public bool TryGetStage(SessionEpoch epoch, out SessionStage stage)
  {
    if (_sessions.TryGetValue(epoch, out SessionRecord? session))
    {
      stage = session.Stage;
      return true;
    }

    stage = SessionStage.Closed;
    return false;
  }

  internal bool HasConnectionAnnouncement(SessionEpoch epoch) {
    return _sessions.TryGetValue(epoch, out SessionRecord? session)
      && session.ConnectionAnnouncementCommitted;
  }

  /// <summary>
  /// Records the prepared announcement in the successful authenticated initial-spawn callback.
  /// Its enclosing session commit advances the revision; this is not a delivery acknowledgement.
  /// </summary>
  internal void RecordConnectionAnnouncement(SessionEpoch epoch) {
    _sessions[epoch].ConnectionAnnouncementCommitted = true;
  }

  public OwnerCommitResult TryTransition(
    SessionEpoch epoch,
    PacketSequence sequence,
    SessionStage expectedStage,
    SessionStage nextStage)
  {
    if (!sequence.IsValid)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "A session transition requires a non-negative packet sequence.");
    }

    if (!Enum.IsDefined(expectedStage) || !Enum.IsDefined(nextStage))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "The requested session transition is not recognized.");
    }

    if (!_sessions.TryGetValue(epoch, out SessionRecord? session))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "Session epoch is not active.");
    }

    if (session.Stage != expectedStage)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "The session is no longer at the expected stage.");
    }

    if (sequence.Value <= session.LastSequence)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "Packet sequence is stale or duplicated.");
    }

    session.Stage = nextStage;
    session.LastSequence = sequence.Value;
    session.Revision++;
    return OwnerCommitResult.Applied(NextCommitVersion());
  }

  /// <summary>
  /// Prepares optional closing output before changing stage, sequence or revision.
  /// The synchronous callback may encode and capture output; it must not mutate inputs or
  /// owners, perform I/O or reenter session handling. Rejection may leave version gaps.
  /// </summary>
  public OwnerCommitResult BeginClosing(
    SessionEpoch epoch,
    PacketSequence sequence,
    Func<CommitVersion, OwnerCommitResult>? prepareOutput = null)
  {
    if (!sequence.IsValid)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "A close transition requires a non-negative packet sequence.");
    }

    if (!_sessions.TryGetValue(epoch, out SessionRecord? session))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "Session epoch is not active.");
    }

    if (session.Stage is SessionStage.Closing or SessionStage.Closed)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The session has already started closing.");
    }

    CommitVersion version = AllocateCommitVersion();
    OwnerCommitResult? outputValidation = prepareOutput?.Invoke(version);
    if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
      return outputValidation;
    }

    if (sequence.Value > session.LastSequence)
    {
      session.LastSequence = sequence.Value;
    }

    session.Stage = SessionStage.Closing;
    session.Revision++;
    return OwnerCommitResult.Applied(version);
  }

  public OwnerCommitResult CompleteClosing(SessionEpoch epoch)
  {
    if (!_sessions.TryGetValue(epoch, out SessionRecord? session))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "Session epoch is not active.");
    }

    if (session.Stage == SessionStage.Closed)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The session is already closed.");
    }

    if (session.Stage != SessionStage.Closing)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "The session must enter Closing before it can be closed.");
    }

    session.ConnectionAnnouncementCommitted = false;
    session.Stage = SessionStage.Closed;
    session.Revision++;
    return OwnerCommitResult.Applied(NextCommitVersion());
  }

  public SessionAdmissionPreparation PrepareHello(
    InboundPacketEnvelope envelope,
    HelloPacket packet,
    bool isBanned)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    OwnerCommitResult? validation = ValidateEnvelope(
      envelope,
      HelloMessageId,
      SessionStage.AwaitHello);
    if (validation is not null)
    {
      return SessionAdmissionPreparation.Failed(validation);
    }

    if (isBanned)
    {
      return PrepareKick(envelope, "You are banned from this server.");
    }

    if (!_policy.IgnoreProtocolVersion
        && !string.Equals(packet.Version, _policy.ProtocolVersion, StringComparison.Ordinal))
    {
      return PrepareKick(envelope, $"This server requires {_policy.ProtocolVersion}.");
    }

    if (!HasAvailablePlayerSlot())
    {
      return PrepareKick(envelope, "The server is full.");
    }

    if (_policy.Password is not null)
    {
      return PrepareResponse(
        envelope,
        SessionStage.AwaitPassword,
        RequestPasswordMessageId,
        new RequestPasswordPacket(),
        playerSlot: null,
        closeAfterResponse: false);
    }

    return PreparePlayerAdmission(envelope);
  }

  public SessionAdmissionPreparation PreparePassword(
    InboundPacketEnvelope envelope,
    SendPasswordPacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    OwnerCommitResult? validation = ValidateEnvelope(
      envelope,
      SendPasswordMessageId,
      SessionStage.AwaitPassword);
    if (validation is not null)
    {
      return SessionAdmissionPreparation.Failed(validation);
    }

    if (_policy.Password is not string expectedPassword
      || !SecretEquals(expectedPassword, packet.Password))
    {
      return PrepareKick(envelope, "Incorrect password.");
    }

    return PreparePlayerAdmission(envelope);
  }

  public SessionTransitionCommitResult Commit(
    PreparedSessionTransition transition,
    ReadOnlyMemory<byte>? encodedResponsePayload)
  {
    ArgumentNullException.ThrowIfNull(transition);
    if (!ReferenceEquals(transition.Owner, this) || transition.IsConsumed)
    {
      return SessionTransitionCommitResult.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "Admission proposal is invalid or already consumed."));
    }

    if (transition.ResponseMessageId is not null && encodedResponsePayload is null)
    {
      return SessionTransitionCommitResult.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.NotReady,
          "Encode the prepared response before committing admission."));
    }

    if (!_sessions.TryGetValue(transition.Epoch, out SessionRecord? session)
      || session.Revision != transition.ExpectedRevision
      || session.Stage != transition.ExpectedStage
      || transition.Sequence.Value <= session.LastSequence)
    {
      return SessionTransitionCommitResult.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEpoch,
          "The session changed after admission was prepared."));
    }

    bool createdPlayer = false;
    if (transition.PlayerSlot is byte playerSlot)
    {
      if (!_players.TryEnsurePlayer(
        transition.Epoch,
        out MinimalImplementation.Server.Player.Player? player,
        out createdPlayer)
        || player is null)
      {
        return SessionTransitionCommitResult.Failed(
          OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "The player identity could not be created."));
      }

      if (!_bindings.TryBind(transition.Epoch, playerSlot, player.Handle))
      {
        if (createdPlayer)
        {
          _players.TryRetire(transition.Epoch);
        }

        return SessionTransitionCommitResult.Failed(
          OwnerCommitResult.Rejected(
            ProtocolErrorCode.Conflict,
            "The player slot was claimed before admission committed."));
      }
    }

    session.Stage = transition.NextStage;
    session.LastSequence = transition.Sequence.Value;
    session.Revision++;
    transition.IsConsumed = true;
    CommitVersion version = NextCommitVersion();
    OutboundPacketIntent? response = transition.ResponseMessageId is byte messageId
      ? new OutboundPacketIntent(
        [transition.Epoch],
        ignoreEpoch: null,
        messageId,
        encodedResponsePayload!.Value,
        version)
      : null;

    return new SessionTransitionCommitResult(
      OwnerCommitResult.Applied(version),
      response,
      transition.CloseAfterResponse);
  }

  public OwnerCommitResult ExecuteAuthenticatedPlayerPacket(
    InboundPacketEnvelope envelope,
    byte expectedMessageId,
    IReadOnlyCollection<SessionStage> allowedStages,
    Func<SessionBinding, OwnerCommitResult> commit)
  {
    return ExecuteAuthenticatedPlayerPacketAndTransition(
      envelope,
      expectedMessageId,
      allowedStages,
      nextStage: null,
      commit);
  }

  public OwnerCommitResult ExecuteAuthenticatedPlayerPacketAndTransition(
    InboundPacketEnvelope envelope,
    byte expectedMessageId,
    IReadOnlyCollection<SessionStage> allowedStages,
    SessionStage? nextStage,
    Func<SessionBinding, OwnerCommitResult> commit)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(allowedStages);
    ArgumentNullException.ThrowIfNull(commit);
    if (!_sessions.TryGetValue(envelope.Epoch, out SessionRecord? session))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "Session epoch is not active.");
    }

    if (envelope.MessageId != expectedMessageId)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Envelope message id does not match the handler.");
    }

    if (envelope.Stage != session.Stage.ToString() || !allowedStages.Contains(session.Stage))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Packet is not allowed in the current session stage.");
    }

    if (envelope.ExpectedRevision is long expectedRevision
      && expectedRevision != session.Revision)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The packet was captured against an older session revision.");
    }

    if (envelope.Sequence.Value <= session.LastSequence)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "Packet sequence is stale or duplicated.");
    }

    if (!_bindings.TryGetByEpoch(envelope.Epoch, out SessionBinding? binding) || binding is null)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Session has no authenticated player binding.");
    }

    if (envelope.WorldScope is long worldScope
      && worldScope != binding.PlayerHandle.WorldScope.Value)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The packet belongs to another world scope.");
    }

    OwnerCommitResult result = commit(binding);
    if (result.Status == OwnerCommitStatus.Applied)
    {
      session.LastSequence = envelope.Sequence.Value;
      session.Revision++;
      if (nextStage is SessionStage stage)
      {
        session.Stage = stage;
      }

      NextCommitVersion();
    }
    return result;
  }

  public IReadOnlyList<SessionEpoch> CaptureBoundRecipients(SessionEpoch ignoreEpoch)
  {
    return Array.AsReadOnly(_bindings.CaptureBindings()
      .Where(binding => binding.Epoch != ignoreEpoch
        && _sessions.TryGetValue(binding.Epoch, out SessionRecord? session)
        && session.Stage is not SessionStage.Closing and not SessionStage.Closed)
      .OrderBy(binding => binding.PlayerSlot)
      .Select(binding => binding.Epoch)
      .ToArray());
  }

  public IReadOnlyList<SessionEpoch> CaptureActiveRecipients(SessionEpoch ignoreEpoch)
  {
    return Array.AsReadOnly(_bindings.CaptureBindings()
      .Where(binding => binding.Epoch != ignoreEpoch
        && _sessions.TryGetValue(binding.Epoch, out SessionRecord? session)
        && session.Stage == SessionStage.Active)
      .OrderBy(binding => binding.PlayerSlot)
      .Select(binding => binding.Epoch)
      .ToArray());
  }

  /// <summary>Captures every authenticated session currently in the Active stage.</summary>
  public IReadOnlyList<SessionEpoch> CaptureActiveSessionEpochs()
  {
    return Array.AsReadOnly(_bindings.CaptureBindings()
      .Where(binding => _sessions.TryGetValue(binding.Epoch, out SessionRecord? session)
        && session.Stage == SessionStage.Active)
      .OrderBy(binding => binding.PlayerSlot)
      .Select(binding => binding.Epoch)
      .ToArray());
  }

  public bool TryGetActiveBindingBySlot(byte playerSlot, out SessionBinding? binding)
  {
    if (!_bindings.TryGetBySlot(playerSlot, out binding)
      || binding is null
      || !_sessions.TryGetValue(binding.Epoch, out SessionRecord? session)
      || session.Stage != SessionStage.Active)
    {
      binding = null;
      return false;
    }

    return true;
  }

  private SessionAdmissionPreparation PreparePlayerAdmission(InboundPacketEnvelope envelope)
  {
    if (_bindings.TryGetByEpoch(envelope.Epoch, out _))
    {
      return SessionAdmissionPreparation.Failed(
        OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "Session already has a player binding."));
    }

    if (_bindings.Count >= _policy.MaximumPlayerSlots
      || !_bindings.TryFindAvailableSlot(_policy.MaximumPlayerSlots, out byte playerSlot))
    {
      return PrepareKick(envelope, "The server is full.");
    }

    return PrepareResponse(
      envelope,
      SessionStage.AwaitPlayerData,
      PlayerInfoMessageId,
      new PlayerInfoPacket { Player = playerSlot, Accepted = false },
      playerSlot,
      closeAfterResponse: false);
  }

  private SessionAdmissionPreparation PrepareKick(InboundPacketEnvelope envelope, string message)
  {
    return PrepareResponse(
      envelope,
      SessionStage.Closing,
      KickMessageId,
      new KickPacket { Text = NetworkText.Literal(message) },
      playerSlot: null,
      closeAfterResponse: true);
  }

  private bool HasAvailablePlayerSlot()
  {
    return _bindings.Count < _policy.MaximumPlayerSlots
      && _bindings.TryFindAvailableSlot(_policy.MaximumPlayerSlots, out _);
  }

  private SessionAdmissionPreparation PrepareResponse(
    InboundPacketEnvelope envelope,
    SessionStage nextStage,
    byte messageId,
    object packet,
    byte? playerSlot,
    bool closeAfterResponse)
  {
    SessionRecord session = _sessions[envelope.Epoch];
    return SessionAdmissionPreparation.Ready(new PreparedSessionTransition(
      this,
      envelope.Epoch,
      envelope.Sequence,
      session.Revision,
      session.Stage,
      nextStage,
      messageId,
      packet,
      playerSlot,
      closeAfterResponse));
  }

  private OwnerCommitResult? ValidateEnvelope(
    InboundPacketEnvelope envelope,
    byte expectedMessageId,
    SessionStage expectedStage)
  {
    if (!_sessions.TryGetValue(envelope.Epoch, out SessionRecord? session))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "Session epoch is not active.");
    }

    if (envelope.MessageId != expectedMessageId)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Envelope message id does not match the handler.");
    }

    if (session.Stage != expectedStage || envelope.Stage != expectedStage.ToString())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Packet is not allowed in the current session stage.");
    }

    if (envelope.Sequence.Value <= session.LastSequence)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "Packet sequence is stale or duplicated.");
    }

    return null;
  }

  private CommitVersion NextCommitVersion()
  {
    if (!_hasCommitted)
    {
      _hasCommitted = true;
      _commitVersion = CommitVersion.Initial;
      return _commitVersion;
    }

    _commitVersion = _commitVersion.Next();
    return _commitVersion;
  }

  private static bool SecretEquals(string expected, string? actual)
  {
    if (actual is null)
    {
      return false;
    }

    byte[] expectedBytes = System.Text.Encoding.UTF8.GetBytes(expected);
    byte[] actualBytes = System.Text.Encoding.UTF8.GetBytes(actual);
    try
    {
      return expectedBytes.Length == actualBytes.Length
        && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
          expectedBytes,
          actualBytes);
    }
    finally
    {
      System.Security.Cryptography.CryptographicOperations.ZeroMemory(expectedBytes);
      System.Security.Cryptography.CryptographicOperations.ZeroMemory(actualBytes);
    }
  }

  private sealed class SessionRecord(SessionEpoch epoch)
  {
    public SessionEpoch Epoch { get; } = epoch;
    public SessionStage Stage { get; set; } = SessionStage.AwaitHello;
    public long LastSequence { get; set; } = -1;
    public long Revision { get; set; } = 1;
    public bool ConnectionAnnouncementCommitted { get; set; }
  }
}
