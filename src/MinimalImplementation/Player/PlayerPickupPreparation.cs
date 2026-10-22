using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Prepared P04 participant. Validate is read-only; Commit applies only already prepared
/// Player state. Send Projections after the whole P04 transaction commits successfully.
/// </summary>
public sealed class PlayerPickupPreparation : IWorldItemPickupPreparation {
  private readonly PlayerOwner _players;
  private readonly Player _player;
  private readonly IPlayerPickupFactsCapture _sources;
  private readonly PlayerVitals _vitals;
  private bool _committed;

  internal PlayerPickupPreparation(
      PlayerOwner players, Player player, PlayerSpatialSnapshot spatial,
      IPlayerPickupFactsCapture sources, WorldItemSnapshot item, ulong worldEpoch,
      WorldItemPickupEffect effect, PlayerVitals vitals, CommitVersion version,
      IReadOnlyCollection<SessionPacketProjection> projections) {
    _players = players;
    _player = player;
    _sources = sources;
    _vitals = vitals;
    Spatial = spatial;
    Sources = sources.Facts;
    SourceRevision = sources.SourceRevision;
    ItemHandle = item.Handle;
    ItemRevision = item.Revision;
    WorldEpoch = worldEpoch;
    CanAcceptItem = Sources.CanAcceptItem(item.Item.Type);
    GrabRange = Sources.GetItemGrabRange(item.Item.Type);
    Effect = effect;
    Version = version;
    Projections = Array.AsReadOnly(projections.ToArray());
  }

  public EntityHandle PlayerHandle => Spatial.PlayerHandle;
  public SessionEpoch Epoch => Spatial.Epoch;
  public WorldItemHandle ItemHandle { get; }
  public CommitVersion ItemRevision { get; }
  public ulong WorldEpoch { get; }
  public int Width => Spatial.Width;
  public int Height => Spatial.Height;
  public bool CanAcceptItem { get; }
  public bool VoidVaultEnabled => Sources.VoidVaultEnabled;
  public WorldItemPickupEffect Effect { get; }
  public CommitVersion Version { get; }
  public PlayerSourceRevision SourceRevision { get; }
  public int GrabRange { get; }
  public PlayerSpatialSnapshot Spatial { get; }
  public PlayerPickupSourceFacts Sources { get; }
  public IReadOnlyList<SessionPacketProjection> Projections { get; }

  public OwnerCommitResult Validate() {
    if (_committed || _player.IsDead || _sources.Epoch != Epoch
        || _sources.PlayerHandle != PlayerHandle || _sources.PlayerVersion != Spatial.Version
        || _sources.SourceRevision != SourceRevision || _sources.Facts != Sources
        || Effect != default && (!_players.TryPeekNextCommitVersion(out CommitVersion next)
          || next != Version)) {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The prepared pickup player, source facts or commit sequence changed.");
    }
    OwnerCommitResult spatial = Spatial.Validate();
    if (spatial.Status != OwnerCommitStatus.Applied) {
      return spatial;
    }
    OwnerCommitResult source = _sources.Validate();
    return source.Status == OwnerCommitStatus.Applied ? OwnerCommitResult.Applied(Version) : source;
  }

  public void Commit() {
    if (_committed) {
      return;
    }
    if (Effect != default) {
      _players.CommitPreparedPickup(_player, _vitals, Version);
    }
    _committed = true;
  }
}
