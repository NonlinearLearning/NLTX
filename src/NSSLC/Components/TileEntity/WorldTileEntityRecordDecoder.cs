using System;
using System.Buffers.Binary;
using System.Text;
using Terraria.Item;

namespace Terraria.World;

/// <summary>
/// Decodes the <c>world.tile-entities</c> record stream into <see cref="WorldTileEntityState"/>.
/// </summary>
/// <remarks>
/// Ported from Version4 <c>TileEntity.Read</c> / <c>ReadInner</c>: each record is a
/// <c>byte</c> kind, an <c>int</c> id, an <c>short</c> x, an <c>short</c> y and then the kind's own
/// payload. The four kinds with variable bitsets (display doll, hat rack, kite anchor, critter
/// anchor) are rejected explicitly: guessing their payload lengths would desynchronize the whole
/// stream, so a world containing them fails to load instead of loading corrupted placement data.
/// </remarks>
public static class WorldTileEntityRecordDecoder
{
  /// <summary>Version4 caps the tile entity count at <c>short.MaxValue</c> worth of ids.</summary>
  public const int MaximumEntities = 4000;

  public static bool TryDecode(
      ReadOnlyMemory<byte> records,
      int entityCount,
      out WorldTileEntityState[] entities,
      out string failure)
  {
    entities = [];
    failure = string.Empty;
    if (entityCount < 0 || entityCount > MaximumEntities)
    {
      failure = $"The tile entity count {entityCount} is outside [0, {MaximumEntities}].";
      return false;
    }

    ReadOnlySpan<byte> span = records.Span;
    WorldTileEntityState[] decoded = new WorldTileEntityState[entityCount];
    int offset = 0;
    for (int index = 0; index < entityCount; index++)
    {
      if (span.Length - offset < 9)
      {
        failure = $"Tile entity record {index} is truncated.";
        return false;
      }

      byte kindValue = span[offset];
      if (kindValue > (byte)WorldTileEntityKind.CritterAnchor)
      {
        failure = $"Tile entity record {index} has unknown kind {kindValue}.";
        return false;
      }

      WorldTileEntityKind kind = (WorldTileEntityKind)kindValue;
      int id = BinaryPrimitives.ReadInt32LittleEndian(span[(offset + 1)..]);
      short x = BinaryPrimitives.ReadInt16LittleEndian(span[(offset + 5)..]);
      short y = BinaryPrimitives.ReadInt16LittleEndian(span[(offset + 7)..]);
      offset += 9;

      ItemStack item = default;
      short npcIndex = -1;
      bool sensorOn = false;
      switch (kind)
      {
        case WorldTileEntityKind.TrainingDummy:
          if (!TryReadInt16(span, ref offset, out npcIndex, index, out failure))
          {
            return false;
          }

          break;
        case WorldTileEntityKind.ItemFrame:
        case WorldTileEntityKind.WeaponsRack:
        case WorldTileEntityKind.FoodPlate:
          if (!TryReadStack(span, ref offset, index, out item, out failure))
          {
            return false;
          }

          break;
        case WorldTileEntityKind.LogicSensor:
          if (span.Length - offset < 2)
          {
            failure = $"Tile entity record {index} is truncated.";
            return false;
          }

          offset++;
          sensorOn = span[offset++] != 0;
          break;
        case WorldTileEntityKind.TeleportationPylon:
        case WorldTileEntityKind.DeadCellsDisplayJar:
          break;
        default:
          failure =
              $"Tile entity record {index} uses kind {kind}, whose payload layout is not ported yet.";
          return false;
      }

      decoded[index] = new WorldTileEntityState(
          (short)id,
          kind,
          new WorldTileCoordinate(x, y),
          item,
          npcIndex,
          sensorOn);
    }

    entities = decoded;
    return true;
  }

  private static bool TryReadInt16(
      ReadOnlySpan<byte> span,
      ref int offset,
      out short value,
      int index,
      out string failure)
  {
    value = 0;
    failure = string.Empty;
    if (span.Length - offset < 2)
    {
      failure = $"Tile entity record {index} is truncated.";
      return false;
    }

    value = BinaryPrimitives.ReadInt16LittleEndian(span[offset..]);
    offset += 2;
    return true;
  }

  /// <summary>Reads the shared <c>netDefaults</c> payload: item type, prefix, stack.</summary>
  private static bool TryReadStack(
      ReadOnlySpan<byte> span,
      ref int offset,
      int index,
      out ItemStack stack,
      out string failure)
  {
    stack = default;
    if (!TryReadInt16(span, ref offset, out short networkType, index, out failure))
    {
      return false;
    }

    if (span.Length - offset < 3)
    {
      failure = $"Tile entity record {index} is truncated.";
      return false;
    }

    byte prefix = span[offset++];
    short quantity = BinaryPrimitives.ReadInt16LittleEndian(span[offset..]);
    offset += 2;
    if (quantity <= 0 || !ItemIdRules.TryFromNetworkType(networkType, out ItemId itemId))
    {
      // 未知或不携带物品的展示位：保留位置，清空载荷，与 Version4 netDefaults 的零结果一致。
      return true;
    }

    stack = new ItemStack(itemId, quantity, prefix);
    return true;
  }
}
