using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Items;

namespace Terraria.WorldStorage;

/// <summary>An immutable TileEntity record used at the world file boundary.</summary>
public sealed class TileEntitySnapshot
{
  public TileEntityId Id { get; }

  public TileEntityTypeId Type { get; }

  public TileCoordinate Anchor { get; }

  public IReadOnlyList<ItemState> Items { get; }

  public short NpcIndex { get; }

  public byte LogicCheck { get; }

  public bool LogicOn { get; }

  public byte Pose { get; }

  public short AnchorItemType { get; }

  public TileEntitySnapshot(
    TileEntityId id,
    TileEntityTypeId type,
    TileCoordinate anchor,
    IReadOnlyList<ItemState> items,
    short npcIndex = -1,
    byte logicCheck = 0,
    bool logicOn = false,
    byte pose = 0,
    short anchorItemType = 0)
  {
    ArgumentNullException.ThrowIfNull(items);
    Id = id;
    Type = type;
    Anchor = anchor;
    Items = Array.AsReadOnly(items.ToArray());
    NpcIndex = npcIndex;
    LogicCheck = logicCheck;
    LogicOn = logicOn;
    Pose = pose;
    AnchorItemType = anchorItemType;
  }
}
