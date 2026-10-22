using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>Owner-captured attachments included with a world section baseline.</summary>
public sealed class WorldSectionPacketAttachments
{
  public WorldSectionPacketAttachments(
    CommitVersion version,
    IReadOnlyList<Packet10ChestRecord> chests,
    IReadOnlyList<Packet10SignRecord> signs,
    IReadOnlyList<PacketTileEntityRecord> tileEntities)
  {
    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);
    ArgumentNullException.ThrowIfNull(tileEntities);

    Packet10ChestRecord[] chestCopy = chests.ToArray();
    Packet10SignRecord[] signCopy = signs.ToArray();
    PacketTileEntityRecord[] tileEntityCopy = tileEntities.ToArray();
    if (tileEntityCopy.Any(static entity => entity is null))
    {
      throw new ArgumentException(
        "Tile entity attachments cannot contain null records.",
        nameof(tileEntities));
    }

    Version = version;
    Chests = Array.AsReadOnly(chestCopy
      .OrderBy(static chest => chest.Id)
      .ToArray());
    Signs = Array.AsReadOnly(signCopy
      .OrderBy(static sign => sign.Id)
      .ToArray());
    TileEntities = Array.AsReadOnly(tileEntityCopy
      .OrderBy(static entity => entity.Type)
      .ThenBy(static entity => entity.Id)
      .ToArray());
  }

  public CommitVersion Version { get; }

  public IReadOnlyList<Packet10ChestRecord> Chests { get; }

  public IReadOnlyList<Packet10SignRecord> Signs { get; }

  public IReadOnlyList<PacketTileEntityRecord> TileEntities { get; }
}
