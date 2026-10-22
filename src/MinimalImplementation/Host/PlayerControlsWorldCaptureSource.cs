using System;
using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

/// <summary>Captures bounded wetness and dismount collision facts for packet 13 only.</summary>
public sealed class PlayerControlsWorldCaptureSource : IPlayerRuntimeWorldCaptureSource {
  private readonly ServerLoop _loop;
  private readonly WorldOwner _world;
  private readonly PlayerOwner _players;

  public PlayerControlsWorldCaptureSource(
      ServerLoop loop, WorldOwner world, PlayerOwner players) {
    ArgumentNullException.ThrowIfNull(loop);
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(players);
    _loop = loop;
    _world = world;
    _players = players;
  }

  public IPlayerRuntimeWorldCapture? Capture(
      SessionBinding binding, PlayerEntity player,
      PlayerMovementInput? pendingMovement = null,
      PlayerRuntimeSpatialState? pendingSpatial = null) {
    if (!_loop.IsCurrent || !_world.IsReady || binding.Epoch != player.Epoch ||
        binding.PlayerHandle != player.Handle ||
        !_players.TryGet(player.Handle, out PlayerEntity? current) ||
        !ReferenceEquals(current, player)) {
      return null;
    }

    WorldHeaderCapture header = _world.CaptureWorldHeader();
    PlayerRuntimeSpatialState spatial = pendingSpatial ?? player.RuntimeSpatial;
    PacketVector2 input = (pendingMovement ?? player.Movement).Position;
    var position = new Vector2(input.X, input.Y);
    // Covers both the original collider and every candidate of a 20x42 dismount resize.
    WorldSectionBounds bounds = WorldSpatialSnapshot.GetQueryBounds(header.Header.Geometry,
      position - new Vector2(20, 42), spatial.Width + 40, spatial.Height + 84);
    if (bounds.Width == 0 || bounds.Height == 0) {
      return null;
    }
    WorldSpatialSnapshot tiles = _world.CaptureSpatialSnapshot(bounds);
    bool wet = tiles.GetLiquidContact(position, spatial.Width, spatial.Height).Wet;
    bool canDismount = tiles.TryResizeFromBottomCenter(position,
      spatial.Width, spatial.Height, 20, 42, out Vector2 dismountedPosition);
    return new CaptureSnapshot(this, player, player.Version, tiles.WorldEpoch,
      tiles.Version, wet, canDismount,
      new PacketVector2(dismountedPosition.X, dismountedPosition.Y));
  }

  private OwnerCommitResult Validate(
      PlayerEntity player, CommitVersion playerVersion, ulong worldEpoch,
      CommitVersion worldVersion) {
    if (!_loop.IsCurrent || player.Version != playerVersion ||
        !_players.TryGet(player.Handle, out PlayerEntity? current) ||
        !ReferenceEquals(current, player) ||
        !_world.IsCurrentWorldRevision(worldEpoch, worldVersion)) {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
        ProtocolErrorCode.Conflict, "The controls collision capture is stale.");
    }

    return OwnerCommitResult.Applied(worldVersion);
  }

  private sealed record CaptureSnapshot(
      PlayerControlsWorldCaptureSource Source,
      PlayerEntity Player,
      CommitVersion PlayerVersion,
      ulong WorldEpoch,
      CommitVersion Revision,
      bool Wet,
      bool FitsDismounted,
      PacketVector2 DismountedPlayerPosition) : IPlayerRuntimeWorldCapture {
    public SessionEpoch Epoch => Player.Epoch;
    public EntityHandle PlayerHandle => Player.Handle;
    public bool? IsWet => Wet;
    public bool? CanFitDismountedPlayer => FitsDismounted;
    public bool? CanFitPortableStool => null;
    // Equipment simulation requires a broader capture source than this controls-only adapter.
    public int DayRate => throw new NotSupportedException();
    public bool LanternsUp => throw new NotSupportedException();
    public bool JourneyWorld => throw new NotSupportedException();
    public bool ExpertMode => throw new NotSupportedException();
    public bool MasterMode => throw new NotSupportedException();
    public bool FarPlacementPowerUnlocked => throw new NotSupportedException();
    public bool FarPlacementPowerEnabled => throw new NotSupportedException();

    public OwnerCommitResult Validate() =>
      Source.Validate(Player, PlayerVersion, WorldEpoch, Revision);
  }
}
