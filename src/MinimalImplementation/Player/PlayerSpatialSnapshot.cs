using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Detached geometry with read guards. Validate together with the World snapshot before using
/// a scan result or committing a prepared operation; no validation method writes owners.
/// </summary>
public sealed class PlayerSpatialSnapshot {
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly Player _player;
  private readonly IPlayerSpatialFactsCapture _sources;

  internal PlayerSpatialSnapshot(
      PlayerOwner players, SessionAdmissionCoordinator sessions, SessionBinding binding,
      Player player, IPlayerSpatialFactsCapture sources) {
    _players = players;
    _sessions = sessions;
    _player = player;
    _sources = sources;
    Binding = binding;
    Version = player.Version;
    SourceRevision = sources.SourceRevision;
    Sources = sources.Facts;
    Position = player.Movement.Position;
  }

  public SessionBinding Binding { get; }
  public SessionEpoch Epoch => Binding.Epoch;
  public EntityHandle PlayerHandle => Binding.PlayerHandle;
  public CommitVersion Version { get; }
  public PlayerSourceRevision SourceRevision { get; }
  public PlayerSpatialSourceFacts Sources { get; }
  public PacketVector2 Position { get; }
  public int Width => Sources.Width;
  public int Height => Sources.Height;
  public int HeightOffsetBoost => Sources.HeightOffsetBoost;
  public PacketVector2 Center => new(Position.X + Width * 0.5f, Position.Y + Height * 0.5f);

  public OwnerCommitResult Validate() {
    if (!_sessions.TryGetStage(Epoch, out SessionStage stage) || stage != SessionStage.Active
        || !_sessions.TryGetBinding(Epoch, out SessionBinding? binding) || binding != Binding
        || !_players.TryGet(PlayerHandle, out Player? current)
        || current is null || !ReferenceEquals(current, _player) || current.Version != Version
        || current.Movement.Position != Position
        || _sources.Epoch != Epoch || _sources.PlayerHandle != PlayerHandle
        || _sources.PlayerVersion != Version || _sources.SourceRevision != SourceRevision
        || _sources.Facts != Sources) {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The captured player geometry or session changed.");
    }

    OwnerCommitResult source = _sources.Validate();
    return source.Status == OwnerCommitStatus.Applied ? OwnerCommitResult.Applied(Version) : source;
  }

  /// <summary>Inclusive source LX..HX/LY..HY, converted to World exclusive dimensions.</summary>
  public WorldSectionBounds GetPylonInteractionBounds(WorldGeometry world) {
    return GetInteractionBounds(world, 60, 60);
  }

  /// <summary>Simple caps effective tileRange at 20 before adding caller-supplied TB.</summary>
  public OwnerCommitResult TryGetSimpleInteractionBounds(
      WorldGeometry world, int tileBoost, out WorldSectionBounds bounds) {
    bounds = default;
    if (Sources.TileRangeX is not int x || Sources.TileRangeY is not int y) {
      return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
        "The effective runtime tileRangeX/Y sources are unavailable.");
    }

    long reachX = (long)Math.Min(x, 20) + tileBoost;
    long reachY = (long)Math.Min(y, 20) + tileBoost;
    if (reachX < 0 || reachY < 0) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "The supplied tile boost produces a negative reach.");
    }
    bounds = GetInteractionBounds(world, reachX, reachY);
    return OwnerCommitResult.Applied(Version);
  }

  private WorldSectionBounds GetInteractionBounds(WorldGeometry world, long x, long y) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(world.Width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(world.Height);
    // Keep the source float arithmetic, truncation and ceiling at tile boundaries.
    long lowX = (long)(Position.X / 16f) - x;
    long highX = (long)Math.Ceiling((Position.X + (float)Width) / 16f) - 1 + x;
    long lowY = (long)(Position.Y / 16f) - y;
    long highY = (long)Math.Ceiling((Position.Y + (float)Height) / 16f) - 1 + y;
    int left = (int)Math.Clamp(lowX, 0, world.Width - 1);
    int right = (int)Math.Clamp(highX, 0, world.Width - 1);
    int top = (int)Math.Clamp(lowY, 0, world.Height - 1);
    int bottom = (int)Math.Clamp(highY, 0, world.Height - 1);
    return new WorldSectionBounds(left, top, right - left + 1, bottom - top + 1);
  }
}
