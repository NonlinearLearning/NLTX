using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// Typed TileEntity record decoding for the generated-world composition, WorldFile 319.
/// </summary>
public sealed class WorldFileTileEntityCodec : IWorldTileEntityDecoder {
  public const int MaxEntityCount = 100_000;

  public WorldFileTileEntitySection Encode(
      IReadOnlyList<TileEntitySnapshot> entities,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(entities);
    cancellationToken.ThrowIfCancellationRequested();
    if (entities.Count > MaxEntityCount) {
      throw new InvalidDataException("The tile entity count exceeds the record limit.");
    }
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    foreach (TileEntitySnapshot entity in entities.OrderBy(entity => entity.Id.Value)) {
      cancellationToken.ThrowIfCancellationRequested();
      writer.Write(entity.Type.Value);
      writer.Write(entity.Id.Value);
      writer.Write(checked((short)entity.Anchor.X));
      writer.Write(checked((short)entity.Anchor.Y));
      switch (entity.Type.Value) {
        case 0:
          writer.Write(entity.NpcIndex);
          break;
        case 1:
        case 4:
        case 6:
          RequireItemCount(entity, 1);
          WriteItem(writer, entity.Items[0]);
          break;
        case 2:
          writer.Write(entity.LogicCheck);
          writer.Write(entity.LogicOn);
          break;
        case 3:
          RequireItemCount(entity, 19);
          GetItemFlags(entity.Items, 0, 9, out byte equipmentFlags,
              out bool equipmentExtra);
          GetItemFlags(entity.Items, 9, 9, out byte dyeFlags,
              out bool dyeExtra);
          bool miscExtra = !entity.Items[18].IsEmpty;
          byte extraFlags = (byte)((equipmentExtra ? 2 : 0) | (dyeExtra ? 4 : 0) |
              (miscExtra ? 1 : 0));
          writer.Write(equipmentFlags);
          writer.Write(dyeFlags);
          writer.Write(entity.Pose);
          writer.Write(extraFlags);
          WriteItems(writer, entity.Items, 0, 9, equipmentFlags, equipmentExtra);
          WriteItems(writer, entity.Items, 9, 9, dyeFlags, dyeExtra);
          if (miscExtra) {
            WriteItem(writer, entity.Items[18]);
          }
          break;
        case 5:
          RequireItemCount(entity, 4);
          GetItemFlags(entity.Items, 0, 4, out byte pylonFlags, out _);
          writer.Write(pylonFlags);
          WriteItems(writer, entity.Items, 0, 4, pylonFlags, hasExtra: false);
          break;
        case 7:
        case 8:
        case 9:
        case 10:
          break;
        default:
          throw new InvalidDataException(
              $"Unsupported tile entity type {entity.Type.Value}.");
      }
    }
    if (stream.Length > WorldFileFormatConstants.MaxRawSectionBytes) {
      throw new InvalidDataException("The tile entity payload exceeds the section size limit.");
    }
    cancellationToken.ThrowIfCancellationRequested();
    return new WorldFileTileEntitySection(entities.Count, stream.ToArray());
  }

  public static WorldFileTileEntitySection EncodeGenerated(
      IReadOnlyList<GeneratedTileEntity> entities) {
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    foreach (GeneratedTileEntity entity in entities) {
      byte type = entity.TileType switch {
        378 => 0, 395 => 1, 423 => 2, 470 => 3, 471 => 4, 475 => 5,
        520 => 6, 597 => 7, 698 => 8,
        _ => throw new InvalidDataException($"Unsupported generated tile entity {entity.TileType}.")
      };
      writer.Write(type);
      writer.Write(entity.Id);
      writer.Write(checked((short)entity.X));
      writer.Write(checked((short)entity.Y));
      switch (type) {
        case 0:
          writer.Write((short)-1);
          break;
        case 1:
        case 4:
        case 6:
          writer.Write((short)0);
          writer.Write((byte)0);
          writer.Write((short)0);
          break;
        case 2:
          writer.Write((byte)0);
          writer.Write(false);
          break;
        case 3:
          writer.Write(new byte[4]);
          break;
        case 5:
          writer.Write((byte)0);
          break;
      }
    }
    return new WorldFileTileEntitySection(entities.Count, stream.ToArray());
  }

  public IReadOnlyList<TileEntitySnapshot> Decode(WorldFileTileEntitySection section) {
    if (section.EntityCount > MaxEntityCount ||
        section.SerializedRecords.Length > WorldFileFormatConstants.MaxRawSectionBytes) {
      throw new InvalidDataException("The tile entity payload exceeds the record limit.");
    }
    using var stream = new MemoryStream(section.SerializedRecords.ToArray(), writable: false);
    using var reader = new BinaryReader(stream);
    var resultByAnchor = new Dictionary<TileCoordinate, TileEntitySnapshot>();
    for (int i = 0; i < section.EntityCount; i++) {
      byte type = reader.ReadByte();
      // Version4 LoadTileEntities replaces this persisted ID with the record ordinal.
      _ = reader.ReadInt32();
      var anchor = new TileCoordinate(reader.ReadInt16(), reader.ReadInt16());
      ItemState[] items = Array.Empty<ItemState>();
      short npcIndex = -1;
      byte logic = 0;
      bool on = false;
      byte pose = 0;
      switch (type) {
        case 0:
          npcIndex = reader.ReadInt16();
          break;
        case 1:
        case 4:
        case 6:
          items = [ReadItem(reader)];
          break;
        case 2:
          logic = reader.ReadByte();
          on = reader.ReadBoolean();
          break;
        case 3:
          byte equipment = reader.ReadByte();
          byte dyes = reader.ReadByte();
          pose = reader.ReadByte();
          byte extra = reader.ReadByte();
          items = new ItemState[19];
          ReadSlots(reader, items, 0, equipment | ((extra & 2) << 7), 9);
          ReadSlots(reader, items, 9, dyes | ((extra & 4) << 6), 9);
          ReadSlots(reader, items, 18, extra & 1, 1);
          break;
        case 5:
          byte flags = reader.ReadByte();
          items = new ItemState[4];
          ReadSlots(reader, items, 0, flags, 4);
          break;
        case 7:
          break;
        case 8:
          // The available Version4 Dead Cells jar serializer has no extra-data payload.
          break;
        case 9:
        case 10:
          // Kite and critter anchors inherit the empty leashed-anchor serializer.
          break;
        default:
          throw new InvalidDataException($"Unsupported tile entity type {type}.");
      }
      // A later record replaces the prior entity at the same anchor in Version4.
      resultByAnchor[anchor] = new TileEntitySnapshot(new TileEntityId(i),
          new TileEntityTypeId(type), anchor, items, npcIndex, logic, on, pose);
    }
    if (stream.Position != stream.Length) {
      throw new InvalidDataException("The tile entity payload has trailing bytes.");
    }
    return resultByAnchor.Values.OrderBy(entity => entity.Id.Value).ToArray();
  }

  private static void ReadSlots(BinaryReader reader, ItemState[] items, int offset, int flags,
      int count) {
    for (int slot = 0; slot < count; slot++) {
      if ((flags & (1 << slot)) != 0) {
        items[offset + slot] = ReadItem(reader);
      }
    }
  }

  private static ItemState ReadItem(BinaryReader reader) {
    int type = reader.ReadInt16();
    byte prefix = reader.ReadByte();
    int stack = reader.ReadInt16();
    return new ItemState(type, prefix, stack);
  }

  private static void RequireItemCount(TileEntitySnapshot entity, int count) {
    if (entity.Items.Count != count) {
      throw new InvalidDataException(
          $"Tile entity {entity.Id.Value} type {entity.Type.Value} requires {count} item slots.");
    }
  }

  private static void GetItemFlags(IReadOnlyList<ItemState> items,
      int offset, int count, out byte flags, out bool hasExtra) {
    flags = 0;
    hasExtra = false;
    for (int index = 0; index < count; index++) {
      if (items[offset + index].IsEmpty) {
        continue;
      }
      if (index < 8) {
        flags |= (byte)(1 << index);
      } else {
        hasExtra = true;
      }
    }
  }

  private static void WriteItems(BinaryWriter writer, IReadOnlyList<ItemState> items,
      int offset, int count, byte flags, bool hasExtra) {
    for (int index = 0; index < count; index++) {
      bool isPresent = index < 8
        ? (flags & (1 << index)) != 0
        : hasExtra;
      if (isPresent) {
        WriteItem(writer, items[offset + index]);
      }
    }
  }

  private static void WriteItem(BinaryWriter writer, ItemState item) {
    if (item.IsEmpty) {
      writer.Write((short)0);
      writer.Write((byte)0);
      writer.Write((short)0);
      return;
    }
    if (item.Type == 0 || item.Stack <= 0 ||
        item.Type < short.MinValue || item.Type > short.MaxValue ||
        item.Stack > short.MaxValue) {
      throw new InvalidDataException("A non-empty tile entity item has an invalid type or stack.");
    }
    writer.Write(checked((short)item.Type));
    writer.Write(item.Prefix);
    writer.Write(checked((short)item.Stack));
  }
}
