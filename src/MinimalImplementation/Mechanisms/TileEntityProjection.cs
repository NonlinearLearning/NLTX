using Terraria.Items;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public sealed class TileEntityProjection
{
  private readonly long _worldScope;

  public long WorldScope => _worldScope;

  public TileEntityProjection(long worldScope)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public PacketTileEntityRecord ForSection(TileEntityRuntimeState state)
  {
    ValidateState(state);
    return CreateRecord(
      state.Definition.Type,
      state.NetworkId,
      state.Anchor,
      state.Items,
      state.NpcIndex,
      state.LogicCheck,
      state.LogicOn,
      state.Pose,
      state.AnchorItemType,
      networkSend: false);
  }

  public PacketTileEntityRecord ForSharing(TileEntityRuntimeState state)
  {
    ValidateState(state);
    return CreateRecord(
      state.Definition.Type,
      null,
      state.Anchor,
      state.Items,
      state.NpcIndex,
      state.LogicCheck,
      state.LogicOn,
      state.Pose,
      state.AnchorItemType,
      networkSend: true);
  }

  public TileEntitySharingPacket CreateSharingMessage(TileEntityRuntimeState state)
  {
    ValidateState(state);
    return new TileEntitySharingPacket
    {
      EntityId = state.NetworkId,
      Exists = true,
      Entity = ForSharing(state)
    };
  }

  public TileEntitySharingPacket CreateRemovalMessage(TileEntityRemovalResult removal)
  {
    ArgumentNullException.ThrowIfNull(removal);
    if (removal.Handle.WorldScope != _worldScope ||
        removal.State.Handle != removal.Handle ||
        removal.State.Lifecycle != MechanismEntityLifecycle.Retired)
    {
      throw new InvalidOperationException(
        "Only a completed removal from this world scope can be projected.");
    }
    return new TileEntitySharingPacket
    {
      EntityId = removal.State.NetworkId,
      Exists = false,
      Entity = null
    };
  }

  /// <summary>Projects a prepared removal for pre-commit encoding without claiming it is retired.</summary>
  public TileEntitySharingPacket CreateRemovalMessage(
    TileEntityRegistry.RemovalPreparation preparation)
  {
    ArgumentNullException.ThrowIfNull(preparation);
    TileEntityRemovalResult removal = preparation.Result;
    if (removal.Handle.WorldScope != _worldScope ||
        removal.State.Handle != removal.Handle ||
        removal.State.Lifecycle != MechanismEntityLifecycle.Active)
    {
      throw new InvalidOperationException(
        "Only an active prepared removal from this world scope can be projected.");
    }
    return new TileEntitySharingPacket
    {
      EntityId = removal.State.NetworkId,
      Exists = false,
      Entity = null
    };
  }

  public IReadOnlyList<PacketTileEntityRecord> ForSection(
    IReadOnlyList<TileEntityRuntimeState> states)
  {
    ArgumentNullException.ThrowIfNull(states);
    PacketTileEntityRecord[] records = states.Select(state => ForSection(state)).ToArray();
    return Array.AsReadOnly(records);
  }

  public IReadOnlyList<PacketTileEntityRecord> ForSection(MechanismSectionSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.CommitVersion.IsValid || snapshot.WorldScope != _worldScope)
    {
      throw new InvalidOperationException(
        "The section snapshot is invalid or belongs to another world scope.");
    }
    PacketTileEntityRecord[] records = snapshot.TileEntities
      .Select(state =>
      {
        ValidateSnapshot(state);
        return CreateRecord(
          state.Type,
          state.NetworkId,
          state.Anchor,
          state.Persistence.Items,
          state.Persistence.NpcIndex,
          state.Persistence.LogicCheck,
          state.Persistence.LogicOn,
          state.Persistence.Pose,
          state.Persistence.AnchorItemType,
          networkSend: false);
      })
      .ToArray();
    return Array.AsReadOnly(records);
  }

  public PacketTileEntityRecord ForSharing(TileEntityRuntimeSnapshot state)
  {
    ValidateSnapshot(state);
    return CreateRecord(
      state.Type,
      null,
      state.Anchor,
      state.Persistence.Items,
      state.Persistence.NpcIndex,
      state.Persistence.LogicCheck,
      state.Persistence.LogicOn,
      state.Persistence.Pose,
      state.Persistence.AnchorItemType,
      networkSend: true);
  }

  public TileEntitySharingPacket CreateSharingMessage(TileEntityRuntimeSnapshot state)
  {
    ValidateSnapshot(state);
    return new TileEntitySharingPacket
    {
      EntityId = state.NetworkId,
      Exists = true,
      Entity = ForSharing(state)
    };
  }

  private static PacketTileEntityRecord CreateRecord(
    byte type,
    int? networkId,
    TileCoordinate anchor,
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    bool networkSend)
  {
    return new PacketTileEntityRecord(
      type,
      ToNetworkCoordinate(anchor.X, nameof(anchor.X)),
      ToNetworkCoordinate(anchor.Y, nameof(anchor.Y)),
      networkId,
      CreateExtraData(type, items, npcIndex, logicCheck, logicOn, pose,
        anchorItemType, networkSend));
  }

  private static object? CreateExtraData(
    byte type,
    IReadOnlyList<ItemState> items,
    short npcIndex,
    byte logicCheck,
    bool logicOn,
    byte pose,
    short anchorItemType,
    bool networkSend)
  {
    return type switch
    {
      0 => npcIndex,
      1 or 4 or 6 or 8 => ToPacketItem(items[0]),
      2 => networkSend ? null : new PacketLogicSensorData(logicCheck, logicOn),
      3 => CreateDisplayDollData(items, pose),
      5 => CreateHatRackData(items),
      7 => null,
      9 or 10 => anchorItemType,
      _ => throw new InvalidOperationException(
        $"TileEntity type {type} has no Version4 packet projection.")
    };
  }

  private static PacketDisplayDollData CreateDisplayDollData(
    IReadOnlyList<ItemState> items,
    byte pose)
  {
    var equipment = new PacketItemStack?[9];
    var dyes = new PacketItemStack?[9];
    for (int index = 0; index < 9; index++)
    {
      equipment[index] = ToNullablePacketItem(items[index]);
      dyes[index] = ToNullablePacketItem(items[index + 9]);
    }
    PacketItemStack? misc = ToNullablePacketItem(items[18]);
    return new PacketDisplayDollData(equipment, dyes, misc, pose);
  }

  private static PacketHatRackData CreateHatRackData(IReadOnlyList<ItemState> itemSlots)
  {
    var items = new PacketItemStack?[2];
    var dyes = new PacketItemStack?[2];
    for (int index = 0; index < 2; index++)
    {
      items[index] = ToNullablePacketItem(itemSlots[index]);
      dyes[index] = ToNullablePacketItem(itemSlots[index + 2]);
    }
    return new PacketHatRackData(items, dyes);
  }

  private static PacketItemStack ToPacketItem(ItemState item)
  {
    if (item.Type < 0 || item.Type > short.MaxValue ||
        item.Stack < 0 || item.Stack > short.MaxValue ||
        ((item.Type == 0) != (item.Stack == 0)))
    {
      throw new InvalidOperationException("A TileEntity item is outside the Version4 wire range.");
    }
    return new PacketItemStack((short)item.Type, item.Prefix, (short)item.Stack);
  }

  private static PacketItemStack? ToNullablePacketItem(ItemState item)
  {
    return item.IsEmpty ? null : ToPacketItem(item);
  }

  private static short ToNetworkCoordinate(int coordinate, string parameterName)
  {
    if (coordinate < 0 || coordinate > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName,
        "TileEntity coordinates must fit the Version4 Int16 wire fields.");
    }
    return (short)coordinate;
  }

  private void ValidateState(TileEntityRuntimeState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (state.Handle.WorldScope != _worldScope ||
        state.Lifecycle != MechanismEntityLifecycle.Active)
    {
      throw new InvalidOperationException(
        "Only active TileEntities from this world scope can be projected.");
    }
  }

  private void ValidateSnapshot(TileEntityRuntimeSnapshot state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (state.Persistence is null ||
        state.Handle.WorldScope != _worldScope ||
        state.Lifecycle != MechanismEntityLifecycle.Active ||
        state.Persistence.Id.Value != state.PersistenceId ||
        state.Persistence.Type.Value != state.Type ||
        state.Persistence.Anchor != state.Anchor ||
        state.NetworkId < 0)
    {
      throw new InvalidOperationException(
        "Only a consistent active TileEntity snapshot from this world scope can be projected.");
    }
  }
}
