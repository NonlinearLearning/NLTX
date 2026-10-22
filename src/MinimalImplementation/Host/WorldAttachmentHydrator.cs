using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using DomainTileCoordinate = MinimalImplementation.Server.World.TileCoordinate;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Restores the persisted attachment sections into their sole runtime owners before admission.
/// </summary>
/// <remarks>
/// The WorldFile chest and sign records are typed application facts. TileEntity records remain
/// opaque at the document boundary and are decoded only by the reviewed 319 codec before the
/// mechanism owner validates their definitions, anchors and payloads.
/// </remarks>
public sealed class WorldAttachmentHydrator
{
  private const int TileEntityCodecFormatVersion = 319;

  private readonly ChestStore _chests;
  private readonly SignRegistry _signs;
  private readonly TileEntityRegistry _tileEntities;
  private readonly MechanismIdentityAuthority _identities;
  private readonly WorldScopeId _worldScope;
  private readonly WorldFileTileEntityCodec _tileEntityCodec;

  public WorldAttachmentHydrator(
    ChestStore chests,
    SignRegistry signs,
    TileEntityRegistry tileEntities,
    MechanismIdentityAuthority identities,
    WorldScopeId worldScope,
    WorldFileTileEntityCodec? tileEntityCodec = null)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);
    ArgumentNullException.ThrowIfNull(tileEntities);
    ArgumentNullException.ThrowIfNull(identities);
    if (!worldScope.IsValid)
    {
      throw new ArgumentException("A world attachment hydrator requires a valid world scope.",
        nameof(worldScope));
    }

    _chests = chests;
    _signs = signs;
    _tileEntities = tileEntities;
    _identities = identities;
    _worldScope = worldScope;
    _tileEntityCodec = tileEntityCodec ?? new WorldFileTileEntityCodec();
  }

  /// <summary>Hydrates all persisted attachment owners or leaves them unchanged on failure.</summary>
  public void Restore(WorldPersistenceDocument document, WorldSnapshot world)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(world);
    if (world.WorldEpoch != (ulong)_worldScope.Value)
    {
      throw new InvalidOperationException(
        "The attachment world scope does not match the published world epoch.");
    }

    WorldFileChestSection chests = GetSection<WorldFileChestSection>(
      document,
      WorldFileChestSection.SectionId,
      WorldFileChestSection.Empty);
    WorldFileSignSection signs = GetSection<WorldFileSignSection>(
      document,
      WorldFileSignSection.SectionId,
      WorldFileSignSection.Empty);
    WorldFileTileEntitySection tileEntitySection = GetSection(
      document,
      WorldFileTileEntitySection.SectionId,
      new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty));

    if (_chests.CaptureWorld(_worldScope).Count != 0 ||
        _signs.Count != 0 ||
        _tileEntities.Count != 0)
    {
      throw new InvalidOperationException(
        "World attachment hydration requires empty chest, sign and TileEntity owners.");
    }

    ValidateChestAnchors(world, chests);
    if (document.FormatVersion != TileEntityCodecFormatVersion &&
        tileEntitySection.EntityCount != 0)
    {
      throw new InvalidDataException(
        "Non-empty TileEntity persistence requires the WorldFile 319 typed codec.");
    }

    IReadOnlyList<TileEntitySnapshot> tileSnapshots = tileEntitySection.EntityCount == 0
      ? Array.Empty<TileEntitySnapshot>()
      : _tileEntityCodec.Decode(tileEntitySection);
    ValidateTileEntityAnchors(world, tileSnapshots);

    ChestStore.PreparedChestLoad? preparedChests = null;
    PreparedSignLoad? preparedSigns = null;
    PreparedTileEntityLoad? preparedTileEntities = null;
    var createdIdentities = new List<MechanismEntityHandle>(tileSnapshots.Count);
    bool chestsCommitted = false;
    bool signsCommitted = false;
    bool tileEntitiesCommitted = false;

    try
    {
      preparedChests = _chests.PrepareRestore(_worldScope, chests.Chests);
      preparedSigns = _signs.PrepareRestore(world, signs.Signs);

      var candidates = new List<TileEntityRestoreCandidate>(tileSnapshots.Count);
      foreach (TileEntitySnapshot snapshot in tileSnapshots)
      {
        MechanismEntityHandle identity = _identities.Create();
        createdIdentities.Add(identity);
        candidates.Add(new TileEntityRestoreCandidate(
          identity,
          new TileEntityRestoreRequest(
            snapshot,
            new MechanismSectionCoordinate(
              snapshot.Anchor.X / WorldGeometry.SectionWidth,
              snapshot.Anchor.Y / WorldGeometry.SectionHeight),
            ToMechanismTile(world.GetTile(new DomainTileCoordinate(
              snapshot.Anchor.X,
              snapshot.Anchor.Y))))));
      }

      preparedTileEntities = _tileEntities.PrepareRestore(candidates);

      ChestStore.ChestLoadCommitResult chestResult = _chests.CommitRestore(preparedChests);
      if (chestResult.Status != ChestStore.ChestLoadCommitStatus.Applied)
      {
        throw new InvalidOperationException("The chest restore became stale before commit.");
      }
      chestsCommitted = true;

      SignLoadResult signResult = _signs.CommitRestore(preparedSigns, world);
      if (signResult.Status != SignLoadCommitStatus.Applied)
      {
        throw new InvalidOperationException("The sign restore became stale before commit.");
      }
      signsCommitted = true;

      TileEntityLoadCommitResult tileEntityResult = _tileEntities.CommitRestore(
        preparedTileEntities);
      if (tileEntityResult.Status != TileEntityLoadCommitStatus.Applied)
      {
        throw new InvalidOperationException(
          "The TileEntity restore became stale before commit.");
      }
      tileEntitiesCommitted = true;
    }
    catch
    {
      if (tileEntitiesCommitted || _tileEntities.Count != 0)
      {
        _tileEntities.Clear();
      }
      if (signsCommitted || _signs.Count != 0)
      {
        _signs.ClearForWorldUnload();
      }
      if (chestsCommitted || _chests.CaptureWorld(_worldScope).Count != 0)
      {
        _chests.ClearForWorldUnload(_worldScope);
      }

      RetireUncommittedIdentities(createdIdentities);
      throw;
    }
  }

  private static TSection GetSection<TSection>(
    WorldPersistenceDocument document,
    string sectionId,
    TSection fallback)
    where TSection : notnull
  {
    return document.TryGetSection<TSection>(
        sectionId,
        out WorldLoadSection<TSection> section) &&
      section.IsPresent
        ? section.Value
        : fallback;
  }

  private static void ValidateChestAnchors(
    WorldSnapshot world,
    WorldFileChestSection chests)
  {
    foreach (WorldFileChestRecord chest in chests.Chests)
    {
      if (!world.Header.Geometry.Contains(new DomainTileCoordinate(chest.X, chest.Y)))
      {
        throw new InvalidDataException(
          $"A persisted chest anchor {chest.X},{chest.Y} lies outside the world.");
      }
    }
  }

  private static void ValidateTileEntityAnchors(
    WorldSnapshot world,
    IReadOnlyList<TileEntitySnapshot> snapshots)
  {
    foreach (TileEntitySnapshot snapshot in snapshots)
    {
      if (!world.Header.Geometry.Contains(new DomainTileCoordinate(
            snapshot.Anchor.X,
            snapshot.Anchor.Y)))
      {
        throw new InvalidDataException(
          $"A persisted TileEntity anchor {snapshot.Anchor.X},{snapshot.Anchor.Y} " +
          "lies outside the world.");
      }
    }
  }

  private static MechanismTileSnapshot ToMechanismTile(WorldTile tile)
  {
    return new MechanismTileSnapshot(tile.Active, tile.Type, tile.FrameX, tile.FrameY);
  }

  private void RetireUncommittedIdentities(IReadOnlyList<MechanismEntityHandle> identities)
  {
    MechanismEntityHandle[] active = identities
      .Where(_identities.IsActive)
      .ToArray();
    if (active.Length != 0 && !_identities.TryRetireMany(active))
    {
      throw new InvalidOperationException(
        "The root identity owner could not retire failed attachment identities.");
    }
  }
}
