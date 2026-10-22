using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Modules.Events;

/// <summary>
/// Single writer for invasion lifecycle; no player, NPC or world globals are read.
/// </summary>
public sealed class InvasionState {
  private const int MaximumPendingP05Kills = 1024;
  private readonly long _worldScope;
  private readonly Dictionary<P05NpcDeathKey, (short NpcType, CommitVersion Version)>
      _pendingP05Kills = new();
  private CommitVersion _version = CommitVersion.Initial;
  private CommitVersion _lastP05SourceVersion;
  private CommitVersion _lastMorningSourceVersion;
  private CommitVersion _lastTickResultVersion;
  private CommitVersion _lastWorldEnvironmentRevision;
  private CommitVersion _lastQualifiedPlayersRevision;
  private InvasionEnvironment? _lastAcceptedEnvironment;
  private InvasionEnvironment? _lastTickEnvironment;
  private long _lastGameTick = -1;
  private int _type;
  private int _size;
  private int _sizeStart;
  private int _delay;
  private double _x;
  private int _warningCountdown;

  public InvasionState(long worldScope) {
    if (worldScope <= 0) {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public InvasionSnapshot CaptureSnapshot() {
    return new InvasionSnapshot(
        _worldScope, _version, _type, _size, _sizeStart, _delay, _x, _warningCountdown);
  }

  /// <summary>
  /// World load creates a new scope; transient source cursors/receipts are not persisted.
  /// </summary>
  public static InvasionState Restore(
      long worldScope, CommitVersion version, WorldFileProgressionSection progression,
      WorldFileQuestSection quests) {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(quests);
    if (!version.IsValid || progression.InvasionType is < 0 or > 4
        || progression.InvasionDelay < 0 || progression.InvasionSize < 0
        || quests.InvasionSizeStart < progression.InvasionSize
        || !double.IsFinite(progression.InvasionX)) {
      throw new ArgumentException("The saved invasion state is invalid.", nameof(progression));
    }
    return new InvasionState(worldScope) {
      _version = version,
      _type = progression.InvasionType,
      _size = progression.InvasionSize,
      _sizeStart = quests.InvasionSizeStart,
      _delay = progression.InvasionDelay,
      _x = progression.InvasionX
    };
  }

  public bool CanStart(InvasionEnvironment environment, bool ignoreDelay = false) {
    ValidateEnvironment(environment);
    return environment.WorldScope == _worldScope
        && CheckEnvironmentCursor(environment) == EnvironmentCursorStatus.Accepted
        && _type == 0
        && (_delay == 0 || ignoreDelay) && environment.QualifiedPlayerCount > 0;
  }

  /// <summary>
  /// Matches StartInvasion, which does not itself enforce invasionDelay. The caller authenticates
  /// and deduplicates commands and uses CanStart where the source admission path requires it.
  /// </summary>
  public InvasionCommitResult Start(
      int invasionType, InvasionEnvironment environment, Func<int, int, int> next) {
    ValidateEnvironment(environment);
    ArgumentNullException.ThrowIfNull(next);
    if (environment.WorldScope != _worldScope) {
      return RejectWorld();
    }
    InvasionCommitResult? environmentResult = ValidateEnvironmentCursor(environment);
    if (environmentResult is not null) {
      return environmentResult;
    }
    if (invasionType is < 1 or > 4) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.InvalidLength, "Invasion type must be 1 through 4.");
    }
    if (_type != 0 && _size != 0) {
      RememberEnvironment(environment);
      return InvasionCommitResult.Applied(_version, Array.Empty<InvasionEffect>());
    }
    if (environment.QualifiedPlayerCount == 0) {
      CommitVersion resetVersion = _type != 0 ? _version.Next() : _version;
      InvasionCommitResult resetResult = InvasionCommitResult.Applied(
          resetVersion, Array.Empty<InvasionEffect>());
      _type = 0;
      _version = resetVersion;
      RememberEnvironment(environment);
      return resetResult;
    }

    CommitVersion nextVersion = _version.Next();
    int players = environment.QualifiedPlayerCount;
    int size = invasionType switch {
      3 => 120 + 60 * players,
      4 => 160 + 40 * players,
      _ => 80 + 40 * players
    };
    double x = environment.SpawnTileX - 1;
    if (invasionType != 4) {
      int edge = next.Invoke(0, 2);
      if (edge is < 0 or > 1) {
        throw new InvalidOperationException("The invasion random draw must be 0 or 1.");
      }
      x = edge == 0 ? 0 : environment.MaximumTilesX;
    }

    InvasionCommitResult result = InvasionCommitResult.Applied(nextVersion, new InvasionEffect[] {
      new InvasionEffect.Started(
          invasionType, new InvasionProgressEffect(0, size, (sbyte)(invasionType + 3), 0))
    });
    _type = invasionType;
    _size = size;
    _sizeStart = size;
    _x = x;
    _warningCountdown = invasionType == 4 ? 2 : 0;
    _version = nextVersion;
    RememberEnvironment(environment);
    return result;
  }

  /// <summary>
  /// P03 calls once for each actual morning transition, before natural event selection.
  /// </summary>
  public InvasionCommitResult AdvanceMorning(long worldScope, CommitVersion sourceVersion) {
    if (worldScope != _worldScope) {
      return RejectWorld();
    }
    if (!sourceVersion.IsValid || sourceVersion.Value < _lastMorningSourceVersion.Value) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The invasion morning fact is older than its cursor.");
    }
    if (sourceVersion == _lastMorningSourceVersion) {
      return InvasionCommitResult.Duplicate(_version);
    }
    CommitVersion nextVersion = _delay > 0 ? _version.Next() : _version;
    InvasionCommitResult result = InvasionCommitResult.Applied(
        nextVersion, Array.Empty<InvasionEffect>());
    if (_delay > 0) {
      _delay--;
    }
    _version = nextVersion;
    _lastMorningSourceVersion = sourceVersion;
    return result;
  }

  public InvasionCommitResult Advance(long gameTickIndex, InvasionEnvironment environment) {
    ValidateEnvironment(environment);
    if (environment.WorldScope != _worldScope) {
      return RejectWorld();
    }
    if (gameTickIndex < 0 || gameTickIndex < _lastGameTick) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity, "The invasion tick is older than its cursor.");
    }
    if (gameTickIndex == _lastGameTick) {
      return environment == _lastTickEnvironment
          ? InvasionCommitResult.Duplicate(_lastTickResultVersion)
          : InvasionCommitResult.Rejected(
              ProtocolErrorCode.Conflict, "The invasion tick was retried with changed facts.");
    }

    InvasionCommitResult? environmentResult = ValidateEnvironmentCursor(environment);
    if (environmentResult is not null) {
      return environmentResult;
    }

    int type = _type;
    int delay = _delay;
    double x = _x;
    int warningCountdown = _warningCountdown;
    List<InvasionEffect> effects = new();
    if (type > 0) {
      if (_size <= 0) {
        short achievement = type switch {
          1 => 10,
          2 => 12,
          3 => 11,
          _ => 13
        };
        effects.Add(new InvasionEffect.ClearEvent(type, type - 1, achievement));
        AddWarning(effects, type, _size, x, environment.SpawnTileX);
        type = 0;
        delay = 0;
        effects.Add(new InvasionEffect.SynchronizeWorldData());
      }

      // The source continues this movement even after clearing the event above.
      if (x != environment.SpawnTileX) {
        float step = Math.Max(environment.DayRate, 1);
        if (x > environment.SpawnTileX) {
          x -= step;
          if (x <= environment.SpawnTileX) {
            x = environment.SpawnTileX;
            AddWarning(effects, type, _size, x, environment.SpawnTileX);
          } else {
            warningCountdown--;
          }
        } else {
          x += step;
          if (x >= environment.SpawnTileX) {
            x = environment.SpawnTileX;
            AddWarning(effects, type, _size, x, environment.SpawnTileX);
          } else {
            warningCountdown--;
          }
        }
        if (warningCountdown <= 0) {
          warningCountdown = 3600;
          AddWarning(effects, type, _size, x, environment.SpawnTileX);
        }
      }
    }

    bool changed = type != _type || delay != _delay || x != _x
        || warningCountdown != _warningCountdown;
    CommitVersion nextVersion = changed ? _version.Next() : _version;
    InvasionCommitResult result = InvasionCommitResult.Applied(nextVersion, effects);
    _type = type;
    _delay = delay;
    _x = x;
    _warningCountdown = warningCountdown;
    _version = nextVersion;
    _lastGameTick = gameTickIndex;
    _lastTickEnvironment = environment;
    _lastTickResultVersion = nextVersion;
    RememberEnvironment(environment);
    return result;
  }

  /// <summary>
  /// Consumes every committed P05 death, independently of Bestiary kill-credit gating.
  /// </summary>
  public InvasionCommitResult ApplyP05Kill(P05NpcDeathKey key, short npcType) {
    if (!key.IsValid || npcType < 0) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.InvalidLength, "Invasion kill credit requires a valid death identity.");
    }
    if (key.WorldScope != _worldScope) {
      return RejectWorld();
    }
    if (_pendingP05Kills.TryGetValue(key, out var receipt)) {
      return receipt.NpcType == npcType
          ? InvasionCommitResult.Duplicate(receipt.Version)
          : InvasionCommitResult.Rejected(
              ProtocolErrorCode.Conflict, "The same death changed its NPC type.");
    }
    if (_pendingP05Kills.Keys.Any(pending => pending.NpcId == key.NpcId
        && pending.Generation == key.Generation)) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "An NPC generation already has a different death commit.");
    }
    if (key.SourceCommitVersion.Value <= _lastP05SourceVersion.Value
        || _pendingP05Kills.Count >= MaximumPendingP05Kills) {
      return InvasionCommitResult.Rejected(
          ProtocolErrorCode.Conflict, "The death is outside the invasion receipt window.");
    }

    int group = npcType switch {
      26 or 27 or 28 or 29 or 111 or 471 or 472 => 1,
      143 or 144 or 145 => 2,
      212 or 213 or 214 or 215 or 216 or 252 or 491 or 492 or 662 => 3,
      381 or 382 or 383 or 385 or 386 or 387 or 388 or 389 or 390 or 391 or 394
          or 395 or 520 => 4,
      _ => 0
    };
    int credit = npcType switch {
      216 => 5,
      395 or 491 or 471 => 10,
      472 or 387 => 0,
      _ => 1
    };
    bool contributes = group > 0 && group == _type && credit > 0;
    int nextSize = contributes ? Math.Max(_size - credit, 0) : _size;
    CommitVersion nextVersion = contributes ? _version.Next() : _version;
    InvasionEffect[] effects = contributes
        ? new InvasionEffect[] { new InvasionEffect.ReportProgress(new InvasionProgressEffect(
            _sizeStart - nextSize, _sizeStart, (sbyte)(group + 3), 0)) }
        : Array.Empty<InvasionEffect>();
    InvasionCommitResult result = InvasionCommitResult.Applied(nextVersion, effects);
    _pendingP05Kills.Add(key, (npcType, nextVersion));
    _size = nextSize;
    _version = nextVersion;
    _lastP05SourceVersion = key.SourceCommitVersion;
    return result;
  }

  /// <summary>Release only after P05 acknowledges the original retained death record.</summary>
  public bool AcknowledgeP05Kill(P05NpcDeathKey key) {
    return _pendingP05Kills.Remove(key);
  }

  private static void AddWarning(
      List<InvasionEffect> effects, int type, int size, double x, int spawnTileX) {
    int text = size <= 0
        ? type switch { 2 => 4, 3 => 24, 4 => 42, _ => 0 }
        : x < spawnTileX
            ? type switch { 2 => 5, 3 => 25, 4 => -1, _ => 1 }
            : x > spawnTileX
                ? type switch { 2 => 6, 3 => 26, 4 => -1, _ => 2 }
                : type switch { 2 => 7, 3 => 27, 4 => 41, _ => 3 };
    if (text >= 0) {
      effects.Add(new InvasionEffect.Warning($"LegacyMisc.{text}"));
    }
  }

  private static void ValidateEnvironment(InvasionEnvironment environment) {
    ArgumentNullException.ThrowIfNull(environment);
    if (environment.WorldScope <= 0 || environment.QualifiedPlayerCount is < 0 or > 255
        || environment.MaximumTilesX <= 0 || environment.SpawnTileX < 0
        || environment.SpawnTileX >= environment.MaximumTilesX || environment.DayRate < 0
        || !environment.WorldRevision.IsValid || !environment.QualifiedPlayersRevision.IsValid) {
      throw new ArgumentException("The committed invasion environment is invalid.",
          nameof(environment));
    }
  }

  private InvasionCommitResult? ValidateEnvironmentCursor(InvasionEnvironment environment) {
    EnvironmentCursorStatus status = CheckEnvironmentCursor(environment);
    return status switch {
      EnvironmentCursorStatus.Accepted => null,
      EnvironmentCursorStatus.Stale => InvasionCommitResult.Rejected(
          ProtocolErrorCode.StaleEntity,
          "The invasion environment is older than the accepted source facts."),
      EnvironmentCursorStatus.Conflict => InvasionCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "The invasion environment changed without a new source revision."),
      _ => throw new InvalidOperationException("The invasion environment cursor is invalid.")
    };
  }

  private EnvironmentCursorStatus CheckEnvironmentCursor(InvasionEnvironment environment) {
    if (environment.WorldRevision.Value < _lastWorldEnvironmentRevision.Value
        || environment.QualifiedPlayersRevision.Value < _lastQualifiedPlayersRevision.Value) {
      return EnvironmentCursorStatus.Stale;
    }

    if (_lastAcceptedEnvironment is InvasionEnvironment previousEnvironment) {
      bool worldFactsChanged = environment.WorldScope != previousEnvironment.WorldScope
          || environment.SpawnTileX != previousEnvironment.SpawnTileX
          || environment.MaximumTilesX != previousEnvironment.MaximumTilesX
          || environment.DayRate != previousEnvironment.DayRate;
      if (environment.WorldRevision == _lastWorldEnvironmentRevision && worldFactsChanged) {
        return EnvironmentCursorStatus.Conflict;
      }

      if (environment.QualifiedPlayersRevision == _lastQualifiedPlayersRevision
          && environment.QualifiedPlayerCount != previousEnvironment.QualifiedPlayerCount) {
        return EnvironmentCursorStatus.Conflict;
      }
    }

    return EnvironmentCursorStatus.Accepted;
  }

  private void RememberEnvironment(InvasionEnvironment environment) {
    _lastWorldEnvironmentRevision = environment.WorldRevision;
    _lastQualifiedPlayersRevision = environment.QualifiedPlayersRevision;
    _lastAcceptedEnvironment = environment;
  }

  private enum EnvironmentCursorStatus {
    Accepted,
    Stale,
    Conflict
  }

  private static InvasionCommitResult RejectWorld() {
    return InvasionCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity, "The invasion fact belongs to another world scope.");
  }
}
