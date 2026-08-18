using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Compatibility;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public readonly record struct NetworkTileEntityItemSlice(
  ushort ItemType,
  byte Prefix,
  short Stack)
{
  public static NetworkTileEntityItemSlice From(LegacyTileEntityItem item)
  {
    return new NetworkTileEntityItemSlice(item.ItemType, item.Prefix, item.Stack);
  }
}

public sealed class NetworkTileEntitySlice
{
  private NetworkTileEntitySlice(
    int entityId,
    short tileX,
    short tileY,
    string entityKind,
    IReadOnlyList<NetworkTileEntityItemSlice> items)
  {
    EntityId = entityId;
    TileX = tileX;
    TileY = tileY;
    EntityKind = entityKind;
    Items = items;
  }

  public int EntityId { get; }

  public short TileX { get; }

  public short TileY { get; }

  public string EntityKind { get; }

  public IReadOnlyList<NetworkTileEntityItemSlice> Items { get; }

  public static NetworkTileEntitySlice From(LegacyTileEntity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    List<NetworkTileEntityItemSlice> items = new();
    string entityKind;
    switch (entity)
    {
      case LegacyTrainingDummyTileEntity:
        entityKind = nameof(LegacyTrainingDummyTileEntity);
        break;
      case LegacyItemTileEntity itemEntity:
        entityKind = itemEntity.Kind.ToString();
        items.Add(NetworkTileEntityItemSlice.From(itemEntity.Item));
        break;
      case LegacyHatRackTileEntity hatRack:
        entityKind = nameof(LegacyHatRackTileEntity);
        Add(items, hatRack.Item0);
        Add(items, hatRack.Item1);
        Add(items, hatRack.Dye0);
        Add(items, hatRack.Dye1);
        break;
      case LegacyDisplayDollTileEntity displayDoll:
        entityKind = nameof(LegacyDisplayDollTileEntity);
        AddRange(items, displayDoll.Equipment);
        AddRange(items, displayDoll.Dyes);
        Add(items, displayDoll.Misc);
        break;
      case LegacyTeleportationPylonTileEntity:
        entityKind = nameof(LegacyTeleportationPylonTileEntity);
        break;
      default:
        throw new InvalidOperationException(
          $"Tile entity type {entity.GetType().Name} is not supported.");
    }

    return new NetworkTileEntitySlice(
      entity.EntityId,
      entity.TileX,
      entity.TileY,
      entityKind,
      items.ToArray());
  }

  private static void Add(
    List<NetworkTileEntityItemSlice> items,
    LegacyTileEntityItem? item)
  {
    if (item is LegacyTileEntityItem value)
    {
      items.Add(NetworkTileEntityItemSlice.From(value));
    }
  }

  private static void AddRange(
    List<NetworkTileEntityItemSlice> items,
    IReadOnlyList<LegacyTileEntityItem?> source)
  {
    for (int index = 0; index < source.Count; index++)
    {
      Add(items, source[index]);
    }
  }
}
