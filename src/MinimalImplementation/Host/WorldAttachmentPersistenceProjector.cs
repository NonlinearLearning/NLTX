using System.IO;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Mechanisms;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects current attachment owners back into a detached persistence document.</summary>
public sealed class WorldAttachmentPersistenceProjector
{
  private readonly WorldFileTileEntityCodec _tileEntityCodec;

  public WorldAttachmentPersistenceProjector(WorldFileTileEntityCodec? tileEntityCodec = null)
  {
    _tileEntityCodec = tileEntityCodec ?? new WorldFileTileEntityCodec();
  }

  public WorldPersistenceDocument Project(
    WorldPersistenceDocument document,
    WorldScopeId worldScope,
    ChestStore chests,
    SignRegistry signs,
    TileEntityRegistry tileEntities)
  {
    ArgumentNullException.ThrowIfNull(document);
    if (!worldScope.IsValid)
    {
      throw new ArgumentException("Attachment persistence requires a valid world scope.",
        nameof(worldScope));
    }
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(signs);
    ArgumentNullException.ThrowIfNull(tileEntities);

    ChestSnapshot[] chestSnapshots = chests.CaptureWorld(worldScope).ToArray();
    var chestRecords = new WorldFileChestRecord[chestSnapshots.Length];
    for (int index = 0; index < chestSnapshots.Length; index++)
    {
      ChestSnapshot snapshot = chestSnapshots[index];
      var items = new WorldFileChestItem[snapshot.Slots.Count];
      for (int slotIndex = 0; slotIndex < snapshot.Slots.Count; slotIndex++)
      {
        InventoryItemState item = snapshot.Slots[slotIndex].Item;
        if (item.Type < 0 || item.Stack < 0 ||
            (item.Type == 0) != (item.Stack == 0))
        {
          throw new InvalidDataException(
            $"Chest {snapshot.Id.Value} contains an invalid item at slot {slotIndex}.");
        }

        items[slotIndex] = new WorldFileChestItem(item.Stack, item.Type, item.Prefix);
      }

      chestRecords[index] = new WorldFileChestRecord(
        snapshot.Position.X,
        snapshot.Position.Y,
        snapshot.Name,
        items);
    }

    WorldFileTileEntitySection tileEntitySection = _tileEntityCodec.Encode(
      tileEntities.CapturePersistence());
    return document.WithReplacements(
      WorldPersistenceSection.Create(
        WorldFileChestSection.SectionId,
        new WorldFileChestSection(chestRecords)),
      WorldPersistenceSection.Create(
        WorldFileSignSection.SectionId,
        new WorldFileSignSection(signs.CapturePersistence())),
      WorldPersistenceSection.Create(
        WorldFileTileEntitySection.SectionId,
        tileEntitySection));
  }
}
