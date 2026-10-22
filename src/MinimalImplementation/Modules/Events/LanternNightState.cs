using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Events;

/// <summary>Server-only Lantern Night owner; all methods run in the commit window.</summary>
public sealed class LanternNightState {
  private CommitVersion _version = CommitVersion.Initial;
  private bool _manual;
  private bool _genuine;
  private bool _nextNightIsLanternNight;
  private int _cooldown;
  private bool _wasLanternNight;
  private long _lastGameTick = -1;
  private LanternNightTimeChange _lastTickTimeChange;
  private LanternNightEnvironment? _lastTickEnvironment;

  public LanternNightSnapshot CaptureSnapshot() {
    return new LanternNightSnapshot(
        _version, _manual, _genuine, _nextNightIsLanternNight, _cooldown);
  }

  public static LanternNightState Restore(
      WorldFileEventSection section, CommitVersion version) {
    ArgumentNullException.ThrowIfNull(section);
    if (!version.IsValid) {
      throw new ArgumentOutOfRangeException(nameof(version));
    }
    return new LanternNightState {
      _version = version,
      _manual = section.LanternNightManual,
      _genuine = section.LanternNightGenuine,
      _nextNightIsLanternNight = section.LanternNightNextNightIsGenuine,
      _cooldown = section.LanternNightCooldown
    };
  }

  /// <summary>Call from P05's committed first-clear fact, not from a client event id.</summary>
  public WorldEventCommitResult RecordFirstEventClear(int gameEventId) {
    if (gameEventId is 4 or 21 or 22 || _nextNightIsLanternNight) {
      return WorldEventCommitResult.Applied(_version, null);
    }
    CommitVersion nextVersion = _version.Next();
    _nextNightIsLanternNight = true;
    _version = nextVersion;
    return WorldEventCommitResult.Applied(_version, null);
  }

  /// <summary>The caller authenticates and deduplicates the accepted manual command.</summary>
  public WorldEventCommitResult ToggleManualLanterns() {
    bool previousLanternsUp = _manual || _genuine;
    CommitVersion nextVersion = _version.Next();
    _manual = !_manual;
    _version = nextVersion;
    bool lanternsUp = _manual || _genuine;
    return WorldEventCommitResult.Applied(
        _version, previousLanternsUp != lanternsUp
            ? new LanternNightChangedEffect(lanternsUp) : null);
  }

  /// <summary>
  /// next(min, max) uses an exclusive max and is called only when source rules draw.
  /// </summary>
  public WorldEventCommitResult Advance(
      long gameTickIndex, LanternNightTimeChange timeChange,
      LanternNightEnvironment environment, Func<int, int, int> next) {
    ArgumentNullException.ThrowIfNull(environment);
    ArgumentNullException.ThrowIfNull(next);
    if (gameTickIndex < 0 || gameTickIndex < _lastGameTick) {
      return WorldEventCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The Lantern tick is older than the owner cursor.");
    }
    if (gameTickIndex == _lastGameTick) {
      if (timeChange != _lastTickTimeChange || environment != _lastTickEnvironment) {
        return WorldEventCommitResult.Rejected(
            ProtocolErrorCode.Conflict, "The Lantern tick was retried with changed facts.");
      }
      return WorldEventCommitResult.Applied(_version, null).AsDuplicate();
    }
    if (timeChange is not (LanternNightTimeChange.None or LanternNightTimeChange.Morning
        or LanternNightTimeChange.Night)
        || (timeChange == LanternNightTimeChange.Morning && !environment.DayTime)
        || (timeChange == LanternNightTimeChange.Night && environment.DayTime)) {
      throw new ArgumentException(
          "The Lantern transition must match the committed P03 day/night fact.",
          nameof(timeChange));
    }

    bool manual = _manual;
    bool genuine = _genuine;
    bool scheduled = _nextNightIsLanternNight;
    int cooldown = _cooldown;
    if (timeChange == LanternNightTimeChange.Morning) {
      manual = false;
      genuine = false;
    } else if (timeChange == LanternNightTimeChange.Night && environment.CanStart) {
      if (cooldown > 0) {
        cooldown--;
      }
      bool start = cooldown == 0 && environment.MoonLordDefeated
          && Draw(next, 0, 14) == 0;
      if (!start && scheduled) {
        scheduled = false;
        start = true;
      }
      if (start) {
        genuine = true;
        cooldown = Draw(next, 5, 11);
      }
    }
    if (genuine && (environment.DayTime || !environment.CanStart)) {
      genuine = false;
    }

    bool changed = manual != _manual || genuine != _genuine
        || scheduled != _nextNightIsLanternNight || cooldown != _cooldown;
    CommitVersion nextVersion = changed ? _version.Next() : _version;
    bool lanternsUp = manual || genuine;
    bool requiresSync = _wasLanternNight != lanternsUp;
    _manual = manual;
    _genuine = genuine;
    _nextNightIsLanternNight = scheduled;
    _cooldown = cooldown;
    _version = nextVersion;
    _lastGameTick = gameTickIndex;
    _lastTickTimeChange = timeChange;
    _lastTickEnvironment = environment;
    _wasLanternNight = lanternsUp;
    return WorldEventCommitResult.Applied(
        _version, requiresSync ? new LanternNightChangedEffect(lanternsUp) : null);
  }

  private static int Draw(Func<int, int, int> next, int minimum, int maximum) {
    int value = next.Invoke(minimum, maximum);
    if (value < minimum || value >= maximum) {
      throw new InvalidOperationException(
          "The Lantern random provider returned a value outside the requested range.");
    }
    return value;
  }
}
