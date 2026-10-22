using System.Text;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Events;

public enum AnglerQuestOutcomeStatus
{
  Applied,
  Duplicate,
  Rejected
}

public sealed record AnglerQuestPublishRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage,
  string WorldDayKey,
  byte QuestId,
  bool Finished);

public sealed record AnglerQuestFinishRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage,
  string WorldDayKey,
  string PlayerName);

public abstract record AnglerQuestEffect(byte MessageId);

public sealed record AnglerQuestPublishedEffect(byte QuestId, bool Finished)
  : AnglerQuestEffect(74);

public sealed record AnglerQuestFinishedEffect(string WorldDayKey, string PlayerName)
  : AnglerQuestEffect(75);

public sealed record AnglerQuestCommitResult(
  AnglerQuestOutcomeStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  string? Detail,
  AnglerQuestEffect? Effect)
{
  public static AnglerQuestCommitResult Applied(
    CommitVersion version,
    AnglerQuestEffect? effect)
  {
    return new AnglerQuestCommitResult(
      AnglerQuestOutcomeStatus.Applied,
      version,
      null,
      null,
      effect);
  }

  public static AnglerQuestCommitResult Rejected(
    ProtocolErrorCode errorCode,
    string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new AnglerQuestCommitResult(
      AnglerQuestOutcomeStatus.Rejected,
      null,
      errorCode,
      detail,
      null);
  }

  public AnglerQuestCommitResult AsDuplicate()
  {
    return Status == AnglerQuestOutcomeStatus.Rejected
      ? this
      : this with { Status = AnglerQuestOutcomeStatus.Duplicate };
  }
}

public sealed class AnglerQuestSnapshot
{
  internal const int MaximumPlayerNameUtf8Bytes = 256;
  internal const int MaximumWorldDayKeyUtf8Bytes = 128;

  public AnglerQuestSnapshot(
    CommitVersion version,
    string worldDayKey,
    byte questId,
    bool finished,
    IReadOnlyCollection<string> completedPlayerNames,
    CommitVersion lastDaySourceVersion = default)
  {
    if (!version.IsValid)
    {
      throw new ArgumentException(
        "An Angler quest snapshot requires a valid commit version.",
        nameof(version));
    }

    ValidateWorldDayKey(worldDayKey);
    ArgumentNullException.ThrowIfNull(completedPlayerNames);
    HashSet<string> uniqueNames = new(StringComparer.Ordinal);
    foreach (string playerName in completedPlayerNames)
    {
      ValidatePlayerName(playerName);
      if (!uniqueNames.Add(playerName))
      {
        throw new ArgumentException(
          "An Angler quest snapshot contains a duplicate player name.",
          nameof(completedPlayerNames));
      }
    }

    Version = version;
    WorldDayKey = worldDayKey;
    QuestId = questId;
    Finished = finished;
    CompletedPlayerNames = Array.AsReadOnly(uniqueNames.Order(StringComparer.Ordinal).ToArray());
    if (lastDaySourceVersion.Value < 0)
    {
      throw new ArgumentException(
        "An Angler quest snapshot cannot contain a negative P03 source version.",
        nameof(lastDaySourceVersion));
    }

    LastDaySourceVersion = lastDaySourceVersion;
  }

  public CommitVersion Version { get; }

  public string WorldDayKey { get; }

  public byte QuestId { get; }

  public bool Finished { get; }

  public IReadOnlyList<string> CompletedPlayerNames { get; }

  public CommitVersion LastDaySourceVersion { get; }

  public bool IsFinishedForPlayer(string playerName) {
    ValidatePlayerName(playerName);
    return CompletedPlayerNames.Contains(playerName, StringComparer.Ordinal);
  }

  public WorldFileQuestSection ToPersistenceSection(WorldFileQuestSection existingSection)
  {
    ArgumentNullException.ThrowIfNull(existingSection);
    return new WorldFileQuestSection(
      CompletedPlayerNames,
      savedAngler: existingSection.SavedAngler,
      anglerQuest: QuestId,
      savedStylist: existingSection.SavedStylist,
      savedTaxCollector: existingSection.SavedTaxCollector,
      savedGolfer: existingSection.SavedGolfer,
      invasionSizeStart: existingSection.InvasionSizeStart,
      cultistDelay: existingSection.CultistDelay);
  }

  internal static void ValidateWorldDayKey(string? worldDayKey)
  {
    ValidateUtf8String(
      worldDayKey,
      MaximumWorldDayKeyUtf8Bytes,
      "The Angler world-day key is invalid.");
  }

  internal static void ValidatePlayerName(string? playerName)
  {
    ValidateUtf8String(
      playerName,
      MaximumPlayerNameUtf8Bytes,
      "The Angler player name is invalid.");
  }

  private static void ValidateUtf8String(
    string? value,
    int maximumBytes,
    string detail)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException(detail);
    }

    try
    {
      if (StrictUtf8.GetByteCount(value) > maximumBytes)
      {
        throw new ArgumentException(detail);
      }
    }
    catch (EncoderFallbackException exception)
    {
      throw new ArgumentException(detail, exception);
    }
  }

  private static readonly UTF8Encoding StrictUtf8 = new(false, true);
}

/// <summary>
/// Owns the Version4 Angler quest publication and per-world-day completion ledger.
/// The player name and day key are immutable facts supplied by P02/P03; this class never reads
/// player, world or persistence globals and only returns typed effects for the root producer.
/// </summary>
public sealed class AnglerQuestState : IModuleEpochRetirable
{
  private sealed record ReplayEntry(object Request, AnglerQuestCommitResult Result);

  private const int MaximumReplayEntries = 4096;

  private readonly Dictionary<ReplayKey, ReplayEntry> _replay = new();
  private readonly Dictionary<SessionEpoch, long> _lastSequenceByEpoch = new();
  private readonly HashSet<string> _completedPlayerNames = new(StringComparer.Ordinal);
  private CommitVersion _version = CommitVersion.Initial;
  private string _worldDayKey = string.Empty;
  private byte _questId;
  private bool _finished;
  private CommitVersion _lastDaySourceVersion;

  public AnglerQuestSnapshot CaptureSnapshot()
  {
    return new AnglerQuestSnapshot(
      _version,
      _worldDayKey,
      _questId,
      _finished,
      _completedPlayerNames,
      _lastDaySourceVersion);
  }

  public AnglerQuestCommitResult AdvanceWorldDay(
    string worldDayKey,
    CommitVersion sourceVersion)
  {
    try
    {
      AnglerQuestSnapshot.ValidateWorldDayKey(worldDayKey);
    }
    catch (ArgumentException exception)
    {
      return AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        exception.Message);
    }

    if (!sourceVersion.IsValid)
    {
      return AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The world-day transition requires a valid P03 source version.");
    }

    if (sourceVersion.Value < _lastDaySourceVersion.Value)
    {
      return AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The world-day transition is older than the last committed P03 day fact.");
    }

    if (sourceVersion == _lastDaySourceVersion
        && !string.Equals(_worldDayKey, worldDayKey, StringComparison.Ordinal))
    {
      return AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "One P03 source revision cannot commit two different world-day keys.");
    }

    if (sourceVersion == _lastDaySourceVersion)
    {
      return AnglerQuestCommitResult.Applied(_version, null).AsDuplicate();
    }

    if (string.Equals(_worldDayKey, worldDayKey, StringComparison.Ordinal))
    {
      _lastDaySourceVersion = sourceVersion;
      return AnglerQuestCommitResult.Applied(_version, null);
    }

    _worldDayKey = worldDayKey;
    _completedPlayerNames.Clear();
    _finished = false;
    CommitVersion nextVersion = _version.Next();
    _version = nextVersion;
    _lastDaySourceVersion = sourceVersion;
    return AnglerQuestCommitResult.Applied(nextVersion, null);
  }

  public AnglerQuestCommitResult Publish(AnglerQuestPublishRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out AnglerQuestCommitResult? replay))
    {
      return replay!;
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Angler quest publication requires an active authenticated session."));
    }

    try
    {
      AnglerQuestSnapshot.ValidateWorldDayKey(request.WorldDayKey);
    }
    catch (ArgumentException exception)
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        exception.Message));
    }

    if (!string.Equals(_worldDayKey, request.WorldDayKey, StringComparison.Ordinal))
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The Angler quest publication uses a world-day key that has not been committed."));
    }

    _questId = request.QuestId;
    _finished = request.Finished;
    CommitVersion nextVersion = _version.Next();
    _version = nextVersion;
    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      AnglerQuestCommitResult.Applied(
        nextVersion,
        new AnglerQuestPublishedEffect(request.QuestId, request.Finished)));
  }

  public AnglerQuestCommitResult RecordFinished(AnglerQuestFinishRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out AnglerQuestCommitResult? replay))
    {
      return replay!;
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Angler quest completion requires an active authenticated session."));
    }

    try
    {
      AnglerQuestSnapshot.ValidateWorldDayKey(request.WorldDayKey);
      AnglerQuestSnapshot.ValidatePlayerName(request.PlayerName);
    }
    catch (ArgumentException exception)
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        exception.Message));
    }

    if (!string.Equals(_worldDayKey, request.WorldDayKey, StringComparison.Ordinal))
    {
      return Remember(request.Epoch, request.Sequence, request, AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The Angler completion uses a world-day key that has not been committed."));
    }

    if (_completedPlayerNames.Contains(request.PlayerName))
    {
      return Remember(
        request.Epoch,
        request.Sequence,
        request,
        AnglerQuestCommitResult.Applied(_version, null) with
        {
          Status = AnglerQuestOutcomeStatus.Duplicate
        });
    }

    _completedPlayerNames.Add(request.PlayerName);
    CommitVersion nextVersion = _version.Next();
    _version = nextVersion;
    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      AnglerQuestCommitResult.Applied(
        nextVersion,
        new AnglerQuestFinishedEffect(request.WorldDayKey, request.PlayerName)));
  }

  public static AnglerQuestState Restore(
    WorldFileQuestSection section,
    string worldDayKey,
    CommitVersion version,
    CommitVersion lastDaySourceVersion = default)
  {
    ArgumentNullException.ThrowIfNull(section);
    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    if (lastDaySourceVersion.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(lastDaySourceVersion));
    }

    AnglerQuestSnapshot.ValidateWorldDayKey(worldDayKey);
    if (section.AnglerQuest < byte.MinValue || section.AnglerQuest > byte.MaxValue)
    {
      throw new InvalidDataException("The persisted Angler quest id is outside the Version4 byte range.");
    }

    HashSet<string> uniqueNames = new(StringComparer.Ordinal);
    foreach (string playerName in section.AnglerWhoFinishedToday)
    {
      AnglerQuestSnapshot.ValidatePlayerName(playerName);
      if (!uniqueNames.Add(playerName))
      {
        throw new InvalidDataException("The persisted Angler completion ledger contains duplicates.");
      }
    }

    AnglerQuestState state = new()
    {
      _version = version,
      _worldDayKey = worldDayKey,
      _questId = (byte)section.AnglerQuest,
      _finished = false,
      _lastDaySourceVersion = lastDaySourceVersion
    };
    state._completedPlayerNames.UnionWith(uniqueNames);
    return state;
  }

  public void RetireEpoch(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      throw new ArgumentException("A valid session epoch is required.", nameof(epoch));
    }

    ReplayKey[] keys = _replay.Keys.Where(key => key.Epoch == epoch).ToArray();
    foreach (ReplayKey key in keys)
    {
      _replay.Remove(key);
    }

    _lastSequenceByEpoch.Remove(epoch);
  }

  private bool TryBeginRequest(
    SessionEpoch epoch,
    PacketSequence sequence,
    object request,
    out AnglerQuestCommitResult? replay)
  {
    if (!epoch.IsValid || !sequence.IsValid)
    {
      replay = AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "An Angler quest request requires a valid epoch and sequence.");
      return false;
    }

    ReplayKey key = new(epoch, sequence);
    if (_replay.TryGetValue(key, out ReplayEntry? cached))
    {
      replay = cached.Request.Equals(request)
        ? cached.Result.AsDuplicate()
        : AnglerQuestCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The Angler quest sequence was reused for a different request.");
      return false;
    }

    if (_lastSequenceByEpoch.TryGetValue(epoch, out long lastSequence)
        && sequence.Value <= lastSequence)
    {
      replay = AnglerQuestCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The Angler quest sequence is older than the active replay window.");
      return false;
    }

    _lastSequenceByEpoch[epoch] = sequence.Value;
    replay = null;
    return true;
  }

  private AnglerQuestCommitResult Remember(
    SessionEpoch epoch,
    PacketSequence sequence,
    object request,
    AnglerQuestCommitResult result)
  {
    if (_replay.Count >= MaximumReplayEntries)
    {
      ReplayKey oldest = _replay.Keys.First();
      _replay.Remove(oldest);
    }

    _replay[new ReplayKey(epoch, sequence)] = new ReplayEntry(request, result);
    return result;
  }

  private readonly record struct ReplayKey(SessionEpoch Epoch, PacketSequence Sequence);
}
