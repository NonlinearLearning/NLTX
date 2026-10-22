using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Host;

/// <summary>Binds persisted teleport facts to one published world on the server loop.</summary>
public sealed class PlayerTeleportWorldCaptureSource {
  private readonly ServerLoop _loop;
  private readonly WorldOwner _world;
  private readonly EntityIdentityRegistry _identities;
  private DocumentBinding? _binding;

  public PlayerTeleportWorldCaptureSource(
    ServerLoop loop, WorldOwner world, EntityIdentityRegistry identities) {
    ArgumentNullException.ThrowIfNull(loop);
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(identities);
    _loop = loop;
    _world = world;
    _identities = identities;
  }

  public void Bind(WorldPersistenceDocument document) {
    ArgumentNullException.ThrowIfNull(document);
    if (!_loop.IsCurrent) {
      throw new InvalidOperationException("Teleport document binding requires the server loop.");
    }

    // Even an unavailable replacement invalidates captures from the previous document binding.
    _binding = null;
    if (!_world.IsReady ||
        !document.TryGetSection(WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> header) || !header.IsPresent ||
        !document.TryGetSection(WorldFileSpawnSection.SectionId,
          out WorldLoadSection<WorldFileSpawnSection> spawn) || !spawn.IsPresent) {
      return;
    }

    WorldHeaderCapture world = _world.CaptureWorldHeader();
    if (!MatchesHeader(header.Value, world.Header)) {
      return;
    }

    _binding = new DocumentBinding(document, new WorldScopeId(_identities.CurrentWorldScope),
      world.WorldEpoch);
  }

  public IPlayerTeleportWorldCapture? Capture(SessionBinding player, WorldHeaderCapture world) {
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(world);
    DocumentBinding? binding = _binding;
    if (!_loop.IsCurrent || binding is null ||
        player.PlayerHandle.WorldScope != binding.WorldScope ||
        binding.WorldScope.Value != _identities.CurrentWorldScope ||
        binding.WorldEpoch != world.WorldEpoch ||
        !_world.IsCurrentWorldRevision(world.WorldEpoch, world.Version) ||
        !binding.Document.TryGetSection(WorldFileHeaderSection.SectionId,
          out WorldLoadSection<WorldFileHeaderSection> header) || !header.IsPresent ||
        !MatchesHeader(header.Value, world.Header) ||
        !binding.Document.TryGetSection(WorldFileSpawnSection.SectionId,
          out WorldLoadSection<WorldFileSpawnSection> spawn) || !spawn.IsPresent) {
      return null;
    }

    return new CaptureSnapshot(this, binding, world.Version, spawn.Value.DualDungeonsSeed);
  }

  private OwnerCommitResult Validate(DocumentBinding binding, CommitVersion version) {
    if (!_loop.IsCurrent || !ReferenceEquals(_binding, binding) ||
        binding.WorldScope.Value != _identities.CurrentWorldScope ||
        !_world.IsCurrentWorldRevision(binding.WorldEpoch, version)) {
      return new OwnerCommitResult(OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict,
        "The teleport document binding, world revision or exclusive server window changed.");
    }

    return OwnerCommitResult.Applied(version);
  }

  private static bool MatchesHeader(WorldFileHeaderSection document, WorldHeaderSnapshot world) {
    return document.UniqueId is Guid uniqueId && uniqueId != Guid.Empty &&
      uniqueId == world.UniqueId && document.WorldId == world.WorldId &&
      document.WorldName == world.Name && document.MaxTilesX == world.Geometry.Width &&
      document.MaxTilesY == world.Geometry.Height;
  }

  private sealed record DocumentBinding(
    WorldPersistenceDocument Document,
    WorldScopeId WorldScope,
    ulong WorldEpoch);

  private sealed class CaptureSnapshot(
    PlayerTeleportWorldCaptureSource source,
    DocumentBinding binding,
    CommitVersion version,
    bool dualDungeonsSeed) : IPlayerTeleportWorldCapture {
    public WorldScopeId WorldScope => binding.WorldScope;
    public ulong WorldEpoch => binding.WorldEpoch;
    public CommitVersion WorldVersion => version;
    public bool DualDungeonsSeed => dualDungeonsSeed;

    public OwnerCommitResult Validate() => source.Validate(binding, version);
  }
}
