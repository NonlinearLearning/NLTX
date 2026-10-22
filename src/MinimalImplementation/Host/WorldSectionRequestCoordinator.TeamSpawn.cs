using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class WorldSectionRequestCoordinator {
  internal sealed class PreparedTeamSpawn : IPlayerTeamSpawnPreparation {
    private readonly WorldSectionRequestCoordinator _owner;
    private readonly WorldTeamSpawnPreparation _world;

    public SessionEpoch Epoch => _world.Epoch;
    public byte Team => _world.Team;
    public bool RequiresExtraSpawn => _world.RequiresExtraSpawn;
    public IReadOnlyList<JoinBatchPacket> SectionPackets { get; }
    internal WorldSectionSendBatch Batch { get; }
    internal IReadOnlyList<WorldSectionCoordinate> RequiredSections => _world.RequiredSections;
    internal bool IsCommitted { get; private set; }

    internal PreparedTeamSpawn(
        WorldSectionRequestCoordinator owner,
        WorldTeamSpawnPreparation world,
        WorldSectionSendBatch batch,
        IReadOnlyCollection<JoinBatchPacket> packets) {
      _owner = owner;
      _world = world;
      Batch = batch;
      SectionPackets = Array.AsReadOnly(packets.ToArray());
    }

    public OwnerCommitResult Validate() {
      if (IsCommitted || !_owner.HasCompletedInitialSync(Epoch) ||
          !_owner.IsCurrentTransfer(Batch.Token)) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEpoch,
          "The team-spawn transfer is no longer current.");
      }

      // P03 stages baselines as Queued at commit. Only the Host ledger proves transport
      // submission; a concurrently pending section must not let this preparation skip it.
      foreach (WorldSectionCoordinate section in RequiredSections) {
        if (!Batch.Sections.Any(group => group.Coordinate == section) &&
            !_owner.HasQueuedSection(Epoch, Batch.Token.WorldEpoch, section)) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            "A required team-spawn section has not completed transport submission.");
        }
      }

      WorldSectionTransferStatus status = _world.Validate();
      return status == WorldSectionTransferStatus.Accepted
        ? OwnerCommitResult.Applied(_world.Version)
        : OwnerCommitResult.Rejected(ProtocolErrorCode.Conflict,
          $"The team-spawn preparation ended with {status}.");
    }

    public void Commit() {
      _world.Commit();
      IsCommitted = true;
    }
  }

  internal PreparedTeamSpawn PrepareTeamSpawn(SessionBinding binding, byte team) {
    if (!HasCompletedInitialSync(binding.Epoch) ||
        !_transferTokens.TryGetValue(binding.Epoch, out WorldSectionTransferToken token)) {
      throw new InvalidOperationException("The session has no completed initial section transfer.");
    }

    if (!_transfers.TryPrepareTeamSpawn(token, team,
          out WorldTeamSpawnPreparation? world, out WorldSectionTransferStatus status) ||
        world is null) {
      throw new InvalidOperationException($"Team-spawn preparation ended with {status}.");
    }

    var sections = new List<WorldSectionSendBatch.Section>(world.Baselines.Count);
    var packets = new List<JoinBatchPacket>();
    foreach (WorldSectionSnapshot snapshot in world.Baselines) {
      sections.Add(EncodeSection(binding.Epoch, snapshot,
        out IReadOnlyList<TileSectionPacket> typed));
      foreach (TileSectionPacket packet in typed) {
        packets.Add(new JoinBatchPacket(10, packet));
      }
    }

    return new PreparedTeamSpawn(this, world, new WorldSectionSendBatch(token, sections), packets);
  }

  internal bool HasQueuedSections(
      WorldSectionTransferToken token, IReadOnlyCollection<WorldSectionCoordinate> sections) {
    return IsCurrentTransfer(token) && sections.All(section =>
      HasQueuedSection(token.SessionEpoch, token.WorldEpoch, section));
  }
}
