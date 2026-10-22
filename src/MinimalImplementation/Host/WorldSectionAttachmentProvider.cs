using System;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Captures Packet 10 attachment facts from the hydrated authoritative owners.
/// </summary>
/// <remarks>
/// The document-only constructor is retained for callers that deliberately exercise the old
/// empty-document boundary. Production composition uses the owner-backed constructor; it never
/// projects a non-empty persistence section as an empty attachment list.
/// </remarks>
public sealed class WorldSectionAttachmentProvider
{
  private readonly WorldPersistenceDocument? _document;
  private readonly ChestStore? _chests;
  private readonly SignRegistry? _signs;
  private readonly TileEntityRegistry? _tileEntities;
  private readonly TileEntityProjection? _tileEntityProjection;
  private readonly WorldScopeId _worldScope;

  public WorldSectionAttachmentProvider(WorldPersistenceDocument document)
  {
    ArgumentNullException.ThrowIfNull(document);
    _document = document;
  }

  public WorldSectionAttachmentProvider(
    WorldPersistenceDocument document,
    ChestStore chests,
    SignRegistry signs,
    TileEntityRegistry tileEntities,
    TileEntityProjection tileEntityProjection,
    WorldScopeId worldScope)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);
    ArgumentNullException.ThrowIfNull(tileEntities);
    ArgumentNullException.ThrowIfNull(tileEntityProjection);
    if (!worldScope.IsValid)
    {
      throw new ArgumentException("A section attachment provider requires a valid world scope.",
        nameof(worldScope));
    }

    _document = document;
    _chests = chests;
    _signs = signs;
    _tileEntities = tileEntities;
    _tileEntityProjection = tileEntityProjection;
    _worldScope = worldScope;
  }

  public WorldSectionPacketAttachments Capture(WorldSectionSnapshot section)
  {
    ArgumentNullException.ThrowIfNull(section);
    if (_chests is null || _signs is null || _tileEntities is null ||
        _tileEntityProjection is null)
    {
      EnsureAttachmentOwnersAreReady();
      return Empty(section.Version);
    }

    if (section.WorldEpoch != (ulong)_worldScope.Value)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 section data belongs to a different published world epoch.");
    }

    EnsureOwnerBackedAttachmentsAreReady();
    ChestSnapshot[] chestSnapshots = _chests
      .CaptureWorld(_worldScope)
      .Where(chest => section.Bounds.Contains(
        new TileCoordinate(chest.Position.X, chest.Position.Y)))
      .ToArray();
    Packet10ChestRecord[] chests = chestSnapshots
      .Select(ToPacketRecord)
      .ToArray();
    IReadOnlyList<Packet10SignRecord> signs = _signs.CaptureSection(section.Bounds);
    IReadOnlyList<PacketTileEntityRecord> tileEntities = _tileEntityProjection.ForSection(
      _tileEntities.GetSection(new MechanismSectionCoordinate(
        section.Coordinate.X,
        section.Coordinate.Y)));
    return new WorldSectionPacketAttachments(section.Version, chests, signs, tileEntities);
  }

  private static WorldSectionPacketAttachments Empty(CommitVersion version)
  {
    return new WorldSectionPacketAttachments(
      version,
      Array.Empty<Packet10ChestRecord>(),
      Array.Empty<Packet10SignRecord>(),
      Array.Empty<PacketTileEntityRecord>());
  }

  private static Packet10ChestRecord ToPacketRecord(ChestSnapshot chest)
  {
    if (chest.Id.Value > short.MaxValue ||
        chest.Position.X < 0 || chest.Position.X > short.MaxValue ||
        chest.Position.Y < 0 || chest.Position.Y > short.MaxValue)
    {
      throw new InvalidOperationException(
        "A persisted chest cannot be represented by Version4 section fields.");
    }

    return new Packet10ChestRecord(
      (short)chest.Id.Value,
      (short)chest.Position.X,
      (short)chest.Position.Y,
      chest.Name);
  }

  private void EnsureAttachmentOwnersAreReady()
  {
    if (_document is null)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 has no persistence document or attachment owners.");
    }

    if (_document.TryGetSection(
          WorldFileChestSection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileChestSection> chests) &&
        chests.IsPresent && chests.Value.Chests.Count != 0)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 requires the authoritative chest owner for non-empty saved chest data.");
    }

    if (_document.TryGetSection(
          WorldFileSignSection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileSignSection> signs) &&
        signs.IsPresent && signs.Value.Signs.Count != 0)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 requires the authoritative sign owner for non-empty saved sign data.");
    }

    if (_document.TryGetSection(
          WorldFileTileEntitySection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileTileEntitySection> tileEntities) &&
        tileEntities.IsPresent &&
        (tileEntities.Value.EntityCount != 0 || !tileEntities.Value.SerializedRecords.IsEmpty))
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 requires the authoritative TileEntity owner for non-empty saved TileEntity data.");
    }
  }

  private void EnsureOwnerBackedAttachmentsAreReady()
  {
    if (_document is null || _chests is null || _signs is null || _tileEntities is null)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "Packet 159 has no complete attachment owner boundary.");
    }

    if (_document.TryGetSection(
          WorldFileChestSection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileChestSection> chests) &&
        chests.IsPresent && chests.Value.Chests.Count != 0 &&
        _chests.CaptureWorld(_worldScope).Count == 0)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "The authoritative chest owner has not hydrated the saved chest data.");
    }

    if (_document.TryGetSection(
          WorldFileSignSection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileSignSection> signs) &&
        signs.IsPresent && signs.Value.Signs.Count != 0 && _signs.Count == 0)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "The authoritative sign owner has not hydrated the saved sign data.");
    }

    if (_document.TryGetSection(
          WorldFileTileEntitySection.SectionId,
          out Terraria.WorldStorage.WorldLoadSection<WorldFileTileEntitySection> tileEntities) &&
        tileEntities.IsPresent &&
        (tileEntities.Value.EntityCount != 0 || !tileEntities.Value.SerializedRecords.IsEmpty) &&
        _tileEntities.Count == 0)
    {
      throw new WorldSectionAttachmentNotReadyException(
        "The authoritative TileEntity owner has not hydrated the saved TileEntity data.");
    }
  }
}

public sealed class WorldSectionAttachmentNotReadyException : InvalidOperationException
{
  public WorldSectionAttachmentNotReadyException(string message)
    : base(message)
  {
  }
}
