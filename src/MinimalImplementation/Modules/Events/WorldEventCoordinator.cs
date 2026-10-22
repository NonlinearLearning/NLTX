using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;

namespace MinimalImplementation.Server.Modules.Events;

public enum WorldEventOutcomeStatus
{
  Applied,
  Duplicate,
  Rejected
}

public enum CrystalEventPhase
{
  Inactive,
  Waiting,
  Active
}

public sealed record WorldEventEnvironment(
  bool WorldLoaded,
  bool IsPumpkinMoon,
  bool IsSnowMoon,
  bool CrystalSpawnBlocked,
  bool CrystalWaitCanBeSkipped,
  int InitialCrystalWaitTicks);

public sealed record TogglePartyRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage,
  bool WorldLoaded);

public sealed record CrystalStartRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage,
  short X,
  short Y);

public sealed record CrystalSkipWaitRequest(
  SessionEpoch Epoch,
  PacketSequence Sequence,
  ModuleSessionStage Stage);

public sealed record CrystalWipeCommit(CommitVersion SourceVersion);

public sealed record InvasionProgressCommit(
  CommitVersion SourceVersion,
  int Progress,
  int ProgressMax,
  sbyte Icon,
  sbyte Wave);

public sealed record TowerShieldCommit(
  CommitVersion SourceVersion,
  ushort Solar,
  ushort Vortex,
  ushort Nebula,
  ushort Stardust);

public abstract record WorldEventEffect(byte MessageId);

public sealed record PartyToggledEffect(bool Enabled) : WorldEventEffect(111);

public sealed record LanternNightChangedEffect(bool LanternsUp) : WorldEventEffect(7);

public sealed record CrystalStartedEffect(short X, short Y, CrystalEventPhase Phase)
  : WorldEventEffect(113);

public sealed record CrystalWaitSkippedEffect() : WorldEventEffect(143);

public sealed record CrystalWaitChangedEffect(int RemainingTicks) : WorldEventEffect(116);

public sealed record CrystalWipedEffect() : WorldEventEffect(114);

public sealed record InvasionProgressEffect(
  int Progress,
  int ProgressMax,
  sbyte Icon,
  sbyte Wave) : WorldEventEffect(78);

public sealed record TowerShieldEffect(
  ushort Solar,
  ushort Vortex,
  ushort Nebula,
  ushort Stardust) : WorldEventEffect(101);

public sealed record WorldEventCommitResult(
  WorldEventOutcomeStatus Status,
  CommitVersion? Version,
  ProtocolErrorCode? ErrorCode,
  string? Detail,
  WorldEventEffect? Effect)
{
  public static WorldEventCommitResult Applied(
    CommitVersion version,
    WorldEventEffect? effect)
  {
    return new WorldEventCommitResult(
      WorldEventOutcomeStatus.Applied,
      version,
      null,
      null,
      effect);
  }

  public static WorldEventCommitResult Rejected(
    ProtocolErrorCode errorCode,
    string detail)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(detail);
    return new WorldEventCommitResult(
      WorldEventOutcomeStatus.Rejected,
      null,
      errorCode,
      detail,
      null);
  }

  public WorldEventCommitResult AsDuplicate()
  {
    return Status == WorldEventOutcomeStatus.Rejected
      ? this
      : this with { Status = WorldEventOutcomeStatus.Duplicate };
  }
}

public sealed record WorldEventSnapshot(
  CommitVersion Version,
  bool PartyEnabled,
  CrystalEventPhase CrystalPhase,
  short? CrystalX,
  short? CrystalY,
  int InvasionProgress,
  int InvasionProgressMax,
  sbyte InvasionIcon,
  sbyte InvasionWave,
  ushort SolarShield,
  ushort VortexShield,
  ushort NebulaShield,
  ushort StardustShield,
  CommitVersion LastInvasionSourceVersion = default,
  CommitVersion LastTowerSourceVersion = default,
  int CrystalWaitTicks = 0,
  long LastCrystalTick = -1,
  CommitVersion LastCrystalSourceVersion = default);

/// <summary>
/// Owns P07's mutable event facts without reading world, player or NPC globals. External owners
/// submit immutable facts/commits; this coordinator validates them, advances one local version and
/// returns typed effects for the root adapter to encode as ordinary packets.
/// </summary>
public sealed class WorldEventCoordinator : IModuleEpochRetirable
{
  // External environment facts are evaluated only on the request's first execution.
  private sealed record ReplayEntry(object Request, WorldEventCommitResult Result);

  private const int MaximumReplayEntries = 4096;
  private const ushort MaximumTowerShield = 100;

  private readonly Dictionary<ReplayKey, ReplayEntry> _replay = new();
  private readonly Dictionary<SessionEpoch, long> _lastSequenceByEpoch = new();
  private CommitVersion _version = CommitVersion.Initial;
  private CommitVersion _lastInvasionSourceVersion;
  private CommitVersion _lastTowerSourceVersion;
  private bool _partyEnabled;
  private CrystalEventPhase _crystalPhase;
  private short? _crystalX;
  private short? _crystalY;
  private int _crystalWaitTicks;
  private long _lastCrystalTick = -1;
  private CommitVersion _lastCrystalSourceVersion;
  private int _invasionProgress;
  private int _invasionProgressMax;
  private sbyte _invasionIcon;
  private sbyte _invasionWave;
  private ushort _solarShield;
  private ushort _vortexShield;
  private ushort _nebulaShield;
  private ushort _stardustShield;

  public WorldEventSnapshot CaptureSnapshot()
  {
    return new WorldEventSnapshot(
      _version,
      _partyEnabled,
      _crystalPhase,
      _crystalX,
      _crystalY,
      _invasionProgress,
      _invasionProgressMax,
      _invasionIcon,
      _invasionWave,
      _solarShield,
      _vortexShield,
      _nebulaShield,
      _stardustShield,
      _lastInvasionSourceVersion,
      _lastTowerSourceVersion,
      _crystalWaitTicks,
      _lastCrystalTick,
      _lastCrystalSourceVersion);
  }

  public static WorldEventCoordinator Restore(WorldEventSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Version.IsValid)
    {
      throw new ArgumentException(
        "A world-event snapshot requires a valid commit version.",
        nameof(snapshot));
    }

    if (snapshot.LastInvasionSourceVersion.Value < 0
        || snapshot.LastTowerSourceVersion.Value < 0
        || snapshot.LastCrystalSourceVersion.Value < 0
        || snapshot.CrystalWaitTicks < 0
        || snapshot.LastCrystalTick < -1)
    {
      throw new ArgumentException(
        "A world-event snapshot cannot contain a negative source version.",
        nameof(snapshot));
    }

    if (!HasValidInvasionProgress(snapshot.InvasionProgress, snapshot.InvasionProgressMax,
        snapshot.InvasionIcon, snapshot.InvasionWave))
    {
      throw new ArgumentException(
        "A world-event snapshot contains an invalid invasion progress state.",
        nameof(snapshot));
    }

    if (snapshot.SolarShield > MaximumTowerShield
        || snapshot.VortexShield > MaximumTowerShield
        || snapshot.NebulaShield > MaximumTowerShield
        || snapshot.StardustShield > MaximumTowerShield)
    {
      throw new ArgumentException(
        "A world-event snapshot contains an invalid tower shield state.",
        nameof(snapshot));
    }

    if (!Enum.IsDefined(snapshot.CrystalPhase)
        || snapshot.CrystalPhase == CrystalEventPhase.Inactive
          && (snapshot.CrystalX is not null || snapshot.CrystalY is not null)
        || snapshot.CrystalPhase != CrystalEventPhase.Inactive
          && (snapshot.CrystalX is null || snapshot.CrystalY is null))
    {
      throw new ArgumentException(
        "A world-event snapshot contains an invalid crystal state.",
        nameof(snapshot));
    }

    if (snapshot.CrystalPhase != CrystalEventPhase.Waiting
        && snapshot.CrystalWaitTicks != 0)
    {
      throw new ArgumentException(
        "A saved crystal wait counter is only valid during the waiting phase.",
        nameof(snapshot));
    }

    WorldEventCoordinator coordinator = new()
    {
      _version = snapshot.Version,
      _partyEnabled = snapshot.PartyEnabled,
      _crystalPhase = snapshot.CrystalPhase,
      _crystalX = snapshot.CrystalX,
      _crystalY = snapshot.CrystalY,
      _invasionProgress = snapshot.InvasionProgress,
      _invasionProgressMax = snapshot.InvasionProgressMax,
      _invasionIcon = snapshot.InvasionIcon,
      _invasionWave = snapshot.InvasionWave,
      _solarShield = snapshot.SolarShield,
      _vortexShield = snapshot.VortexShield,
      _nebulaShield = snapshot.NebulaShield,
      _stardustShield = snapshot.StardustShield,
      _lastInvasionSourceVersion = snapshot.LastInvasionSourceVersion,
      _lastTowerSourceVersion = snapshot.LastTowerSourceVersion,
      _crystalWaitTicks = snapshot.CrystalWaitTicks,
      _lastCrystalTick = snapshot.LastCrystalTick,
      _lastCrystalSourceVersion = snapshot.LastCrystalSourceVersion
    };
    return coordinator;
  }

  public WorldEventCommitResult ToggleParty(TogglePartyRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out WorldEventCommitResult? replay))
    {
      return replay!;
    }

    if (!request.WorldLoaded)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "The world must be loaded before the manual party toggle can commit."));
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "The manual party toggle requires an active authenticated session."));
    }

    _partyEnabled = !_partyEnabled;
    CommitVersion nextVersion = AdvanceVersion();
    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      WorldEventCommitResult.Applied(nextVersion, new PartyToggledEffect(_partyEnabled)));
  }

  public WorldEventCommitResult StartCrystal(
    CrystalStartRequest request,
    WorldEventEnvironment environment)
  {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(environment);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out WorldEventCommitResult? replay))
    {
      return replay!;
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Crystal start requires an active authenticated session."));
    }

    if (!environment.WorldLoaded)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.NotReady,
        "The world must be loaded before a crystal event can start."));
    }

    if (environment.IsPumpkinMoon || environment.IsSnowMoon || environment.CrystalSpawnBlocked)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The world facts reject this crystal event location or phase."));
    }

    if (_crystalPhase != CrystalEventPhase.Inactive)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "A crystal event is already waiting or active."));
    }

    if (environment.InitialCrystalWaitTicks < 0)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The crystal wait duration cannot be negative."));
    }

    _crystalPhase = environment.InitialCrystalWaitTicks == 0
      ? CrystalEventPhase.Active
      : CrystalEventPhase.Waiting;
    _crystalX = request.X;
    _crystalY = request.Y;
    _crystalWaitTicks = environment.InitialCrystalWaitTicks;
    _lastCrystalTick = -1;
    CommitVersion nextVersion = AdvanceVersion();
    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      WorldEventCommitResult.Applied(
        nextVersion,
        new CrystalStartedEffect(request.X, request.Y, _crystalPhase)));
  }

  public WorldEventCommitResult SkipCrystalWait(
    CrystalSkipWaitRequest request,
    bool worldLoaded,
    bool canSkip)
  {
    ArgumentNullException.ThrowIfNull(request);
    if (!TryBeginRequest(
        request.Epoch, request.Sequence, request, out WorldEventCommitResult? replay))
    {
      return replay!;
    }

    if (request.Stage != ModuleSessionStage.Active)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Crystal wait skipping requires an active authenticated session."));
    }

    if (!worldLoaded || !canSkip || _crystalPhase != CrystalEventPhase.Waiting)
    {
      return Remember(request.Epoch, request.Sequence, request, WorldEventCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The current crystal state cannot skip its wait phase."));
    }

    _crystalPhase = CrystalEventPhase.Active;
    _crystalWaitTicks = 0;
    CommitVersion nextVersion = AdvanceVersion();
    return Remember(
      request.Epoch,
      request.Sequence,
      request,
      WorldEventCommitResult.Applied(nextVersion, new CrystalWaitSkippedEffect()));
  }

  public WorldEventCommitResult AdvanceCrystal(long gameTickIndex, int elapsedTicks = 1)
  {
    if (gameTickIndex < 0 || gameTickIndex < _lastCrystalTick)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The crystal tick is older than the owner cursor.");
    }

    if (elapsedTicks <= 0)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The crystal tick must advance by a positive number of ticks.");
    }

    if (gameTickIndex == _lastCrystalTick)
    {
      return WorldEventCommitResult.Applied(_version, null).AsDuplicate();
    }

    _lastCrystalTick = gameTickIndex;
    if (_crystalPhase != CrystalEventPhase.Waiting)
    {
      return WorldEventCommitResult.Applied(_version, null);
    }

    int remaining = Math.Max(_crystalWaitTicks - elapsedTicks, 0);
    _crystalWaitTicks = remaining;
    if (remaining == 0)
    {
      _crystalPhase = CrystalEventPhase.Active;
    }

    CommitVersion nextVersion = _version.Next();
    _version = nextVersion;
    return WorldEventCommitResult.Applied(
      nextVersion,
      new CrystalWaitChangedEffect(remaining));
  }

  public WorldEventCommitResult ApplyCrystalWipe(CrystalWipeCommit commit)
  {
    ArgumentNullException.ThrowIfNull(commit);
    if (!commit.SourceVersion.IsValid)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The crystal wipe source version must be valid.");
    }

    if (commit.SourceVersion == _lastCrystalSourceVersion)
    {
      return WorldEventCommitResult.Applied(_version, null).AsDuplicate();
    }

    if (commit.SourceVersion.Value < _lastCrystalSourceVersion.Value)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "The crystal wipe source version is older than the accepted result.");
    }

    _lastCrystalSourceVersion = commit.SourceVersion;
    if (_crystalPhase == CrystalEventPhase.Inactive)
    {
      return WorldEventCommitResult.Applied(_version, null);
    }

    _crystalPhase = CrystalEventPhase.Inactive;
    _crystalX = null;
    _crystalY = null;
    _crystalWaitTicks = 0;
    CommitVersion nextVersion = AdvanceVersion();
    return WorldEventCommitResult.Applied(nextVersion, new CrystalWipedEffect());
  }

  public WorldEventCommitResult ApplyInvasionProgress(InvasionProgressCommit commit)
  {
    ArgumentNullException.ThrowIfNull(commit);
    if (!commit.SourceVersion.IsValid) {
      return WorldEventCommitResult.Rejected(
          ProtocolErrorCode.InvalidLength, "An invasion source version must be valid.");
    }

    if (!HasValidInvasionProgress(commit.Progress, commit.ProgressMax, commit.Icon, commit.Wave))
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The invasion progress commit is outside its bounded Version4 ranges.");
    }

    if (commit.SourceVersion == _lastInvasionSourceVersion) {
      if (commit.Progress != _invasionProgress || commit.ProgressMax != _invasionProgressMax
          || commit.Icon != _invasionIcon || commit.Wave != _invasionWave) {
        return WorldEventCommitResult.Rejected(
            ProtocolErrorCode.Conflict, "The invasion source version has different facts.");
      }

      return new WorldEventCommitResult(
          WorldEventOutcomeStatus.Duplicate, _version, null, null, null);
    }

    if (commit.SourceVersion.Value < _lastInvasionSourceVersion.Value) {
      return WorldEventCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The invasion source version is no longer current.");
    }

    CommitVersion nextVersion = _version.Next();
    _invasionProgress = commit.Progress;
    _invasionProgressMax = commit.ProgressMax;
    _invasionIcon = commit.Icon;
    _invasionWave = commit.Wave;
    _lastInvasionSourceVersion = commit.SourceVersion;
    _version = nextVersion;
    return WorldEventCommitResult.Applied(
      nextVersion,
      new InvasionProgressEffect(
        commit.Progress,
        commit.ProgressMax,
        commit.Icon,
        commit.Wave));
  }

  private static bool HasValidInvasionProgress(int progress, int maximum, sbyte icon, sbyte wave) {
    // Final moon waves legitimately use maximum zero or progress above their threshold.
    return progress >= -1 && maximum >= 0 && icon is >= 0 and <= 7 && wave >= 0;
  }

  public WorldEventCommitResult ApplyTowerShields(TowerShieldCommit commit)
  {
    ArgumentNullException.ThrowIfNull(commit);
    if (!commit.SourceVersion.IsValid)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The tower shield source version must be valid.");
    }

    if (commit.Solar > MaximumTowerShield
        || commit.Vortex > MaximumTowerShield
        || commit.Nebula > MaximumTowerShield
        || commit.Stardust > MaximumTowerShield)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "The tower shield commit exceeds Version4 LunarShieldPowerMax.");
    }

    if (commit.SourceVersion == _lastTowerSourceVersion)
    {
      if (commit.Solar != _solarShield
          || commit.Vortex != _vortexShield
          || commit.Nebula != _nebulaShield
          || commit.Stardust != _stardustShield)
      {
        return WorldEventCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The tower shield source version has different facts.");
      }

      return new WorldEventCommitResult(
        WorldEventOutcomeStatus.Duplicate,
        _version,
        null,
        null,
        null);
    }

    if (commit.SourceVersion.Value < _lastTowerSourceVersion.Value)
    {
      return WorldEventCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The tower shield commit is older than the accepted source version.");
    }

    _solarShield = commit.Solar;
    _vortexShield = commit.Vortex;
    _nebulaShield = commit.Nebula;
    _stardustShield = commit.Stardust;
    _lastTowerSourceVersion = commit.SourceVersion;
    CommitVersion nextVersion = AdvanceVersion();
    return WorldEventCommitResult.Applied(
      nextVersion,
      new TowerShieldEffect(
        commit.Solar,
        commit.Vortex,
        commit.Nebula,
        commit.Stardust));
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
    out WorldEventCommitResult? replay)
  {
    if (!epoch.IsValid || !sequence.IsValid)
    {
      replay = WorldEventCommitResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "An event request requires a valid session epoch and sequence.");
      return false;
    }

    ReplayKey key = new(epoch, sequence);
    if (_replay.TryGetValue(key, out ReplayEntry? cached))
    {
      replay = cached.Request.Equals(request)
        ? cached.Result.AsDuplicate()
        : WorldEventCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The event sequence was reused for a different request.");
      return false;
    }

    if (_lastSequenceByEpoch.TryGetValue(epoch, out long lastSequence)
        && sequence.Value <= lastSequence)
    {
      replay = WorldEventCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "The event sequence is older than the active replay window.");
      return false;
    }

    _lastSequenceByEpoch[epoch] = sequence.Value;
    replay = null;
    return true;
  }

  private WorldEventCommitResult Remember(
    SessionEpoch epoch,
    PacketSequence sequence,
    object request,
    WorldEventCommitResult result)
  {
    if (_replay.Count >= MaximumReplayEntries)
    {
      ReplayKey oldest = _replay.Keys.First();
      _replay.Remove(oldest);
    }

    _replay[new ReplayKey(epoch, sequence)] = new ReplayEntry(request, result);
    return result;
  }

  private CommitVersion AdvanceVersion()
  {
    _version = _version.Next();
    return _version;
  }

  private readonly record struct ReplayKey(SessionEpoch Epoch, PacketSequence Sequence);
}
