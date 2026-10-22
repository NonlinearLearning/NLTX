using static MinimalImplementation.Server.Protocol.Version4ModulePayloads;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Protocol;

/// <summary>Translates the server's logical module IDs to the Terraria 1.4.5.8 Steam wire registry.</summary>
public static class Version4SteamModuleIdMap
{
  public static bool SupportsLogicalModule(ushort logicalModuleId)
  {
    return logicalModuleId < 15 && logicalModuleId != 12;
  }

  public static ushort ToWireModuleId(ushort logicalModuleId)
  {
    if (logicalModuleId == 12)
    {
      throw new NotSupportedException(
        "The Terraria 1.4.5.8 module registry has no slot for the server's custom TagEffect module.");
    }

    if (logicalModuleId >= 15)
    {
      throw new ArgumentOutOfRangeException(nameof(logicalModuleId));
    }

    return logicalModuleId is >= 5 and <= 11
      ? (ushort)(logicalModuleId + 1)
      : logicalModuleId;
  }

  public static ushort ToLogicalModuleId(ushort wireModuleId)
  {
    return Packet82KnownModuleCodecsV4.ResolveLegacySteamModuleId(wireModuleId);
  }
}

public static class Version4ModuleCodecs
{
  private const int MaximumModuleBodyBytes = PacketFrameCodec.MaximumPayloadBytes - sizeof(ushort);

  public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> Create(bool isServer)
  {
    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> standard =
      Packet82KnownModuleCodecsV4.CreateSteam();
    var codecs = new IPacket82ModuleWireCodec[Enum.GetValues<Packet82ModuleId>().Length];
    for (ushort moduleId = 0; moduleId < codecs.Length; moduleId++)
    {
      codecs[moduleId] = moduleId switch
      {
        0 => new ModuleCodec<Packet82LiquidData>(0, WriteLiquid, ReadLiquid),
        1 => new TextModuleCodec(isServer),
        7 => new ModuleCodec<CreativeSacrificeReportData>(
          7, WriteCreativeSacrifice, ReadCreativeSacrifice),
        12 => new CraftingRequestsModuleCodec(isServer, 12),
        13 => new ModuleCodec<LeashedEntityMessage>(13, WriteLeashedEntity, ReadLeashedEntity),
        14 => new UnbreakableWallModuleCodec(isServer),
        _ => standard[moduleId]
      };
    }

    return Packet82KnownModuleCodecsV4.Create(codecs);
  }

  private static void WriteLiquid(PacketWireWriter writer, Packet82LiquidData payload)
  {
    ArgumentNullException.ThrowIfNull(payload.Entries);
    if (payload.Entries.Count > (MaximumModuleBodyBytes - sizeof(ushort)) / 6)
    {
      throw new PacketWireFormatException("Packet 82 liquid entries exceed the maximum frame size.");
    }

    writer.WriteUInt16((ushort)payload.Entries.Count);
    foreach (Packet82LiquidEntry entry in payload.Entries)
    {
      writer.WriteInt32(entry.PackedTileCoordinate);
      writer.WriteByte(entry.Amount);
      writer.WriteByte(entry.LiquidType);
    }
  }

  private static Packet82LiquidData ReadLiquid(PacketWireReader reader)
  {
    int count = reader.ReadUInt16();
    if (count > (MaximumModuleBodyBytes - sizeof(ushort)) / 6
        || reader.Remaining != count * 6)
    {
      throw new PacketWireFormatException(
        "Packet 82 liquid entry count does not match its bounded body or frame capacity.");
    }

    var entries = new Packet82LiquidEntry[count];
    for (int index = 0; index < entries.Length; index++)
    {
      entries[index] = new Packet82LiquidEntry(
        reader.ReadInt32(),
        reader.ReadByte(),
        reader.ReadByte());
    }

    return new Packet82LiquidData(entries);
  }

  private static void WriteCreativeSacrifice(
    PacketWireWriter writer,
    CreativeSacrificeReportData payload)
  {
    writer.WriteByte(payload.Player);
    writer.WriteUInt16(payload.ItemId);
    writer.WriteUInt16(payload.Amount);
  }

  private static CreativeSacrificeReportData ReadCreativeSacrifice(PacketWireReader reader)
  {
    return new CreativeSacrificeReportData(
      reader.ReadByte(),
      reader.ReadUInt16(),
      reader.ReadUInt16());
  }

  private static void WriteTagEffect(
    PacketWireWriter writer,
    TagEffectMessage payload,
    int slotCount,
    Func<short, bool> usesProcTimes)
  {
    ValidateSlotCount(slotCount);
    writer.WriteByte(payload.Player);
    writer.WriteByte(payload.Action);
    switch (payload.Action)
    {
      case 0:
        if (payload.EffectType is not short fullEffectType || payload.NpcIndex is not null)
        {
          throw new PacketWireFormatException("TagEffect full-state data requires an effect type and no NPC index.");
        }

        RequireTimerTable(payload.NpcTimes, slotCount, "tag");
        bool hasProcTimes = usesProcTimes(fullEffectType);
        if (hasProcTimes)
        {
          RequireTimerTable(payload.ProcTimes, slotCount, "proc");
        }
        else if (payload.ProcTimes is not null)
        {
          throw new PacketWireFormatException("This TagEffect message has no proc timer table.");
        }

        writer.WriteInt16(fullEffectType);
        WriteSparseNpcTimes(writer, payload.NpcTimes!);
        if (hasProcTimes)
        {
          WriteSparseNpcTimes(writer, payload.ProcTimes!);
        }

        break;
      case 1:
        if (payload.EffectType is not short effectType
            || payload.NpcTimes is not null
            || payload.ProcTimes is not null
            || payload.NpcIndex is not null)
        {
          throw new PacketWireFormatException("TagEffect effect changes require only an effect type.");
        }

        writer.WriteInt16(effectType);
        break;
      case 2:
      case 3:
      case 4:
        if (payload.EffectType is not null
            || payload.NpcTimes is not null
            || payload.ProcTimes is not null
            || payload.NpcIndex is not byte npcIndex)
        {
          throw new PacketWireFormatException("TagEffect NPC actions require only an NPC slot.");
        }

        writer.WriteByte(npcIndex);
        break;
      default:
        throw new PacketWireFormatException($"Unknown Version4 TagEffect action {payload.Action}.");
    }
  }

  private static TagEffectMessage ReadTagEffect(
    PacketWireReader reader,
    int slotCount,
    Func<short, bool> usesProcTimes)
  {
    ValidateSlotCount(slotCount);
    byte player = reader.ReadByte();
    byte action = reader.ReadByte();
    switch (action)
    {
      case 0:
      {
        short effectType = reader.ReadInt16();
        int[] npcTimes = ReadSparseNpcTimes(reader, slotCount);
        int[]? procTimes = usesProcTimes(effectType)
          ? ReadSparseNpcTimes(reader, slotCount)
          : null;
        return new TagEffectMessage(player, action, effectType, npcTimes, procTimes, null);
      }
      case 1:
        return new TagEffectMessage(player, action, reader.ReadInt16(), null, null, null);
      case 2:
      case 3:
      case 4:
        return new TagEffectMessage(player, action, null, null, null, reader.ReadByte());
      default:
        throw new PacketWireFormatException($"Unknown Version4 TagEffect action {action}.");
    }
  }

  private static void WriteSparseNpcTimes(PacketWireWriter writer, IReadOnlyList<int> times)
  {
    for (int index = 0; index < times.Count; index++)
    {
      if (times[index] == 0)
      {
        continue;
      }

      writer.WriteByte((byte)index);
      writer.WriteInt32(times[index]);
    }

    writer.WriteByte((byte)times.Count);
  }

  private static int[] ReadSparseNpcTimes(PacketWireReader reader, int slotCount)
  {
    byte index = reader.ReadByte();
    var times = new int[slotCount];
    while (index < slotCount)
    {
      times[index] = reader.ReadInt32();
      index = reader.ReadByte();
    }

    return times;
  }

  private static void RequireTimerTable(
    IReadOnlyList<int>? values,
    int slotCount,
    string name)
  {
    if (values is null || values.Count != slotCount)
    {
      throw new PacketWireFormatException(
        $"TagEffect {name} timer table must contain exactly {slotCount} NPC slots.");
    }
  }

  private static void ValidateSlotCount(int slotCount)
  {
    if (slotCount is < 0 or > byte.MaxValue)
    {
      throw new PacketWireFormatException("TagEffect NPC slot count must fit its byte terminator.");
    }
  }

  private static void WriteLeashedEntity(PacketWireWriter writer, LeashedEntityMessage payload)
  {
    if (payload.Action == 0)
    {
      RequireRemoveShape(payload);
      if (payload.Slot < 0)
      {
        throw new PacketWireFormatException("Leashed entity slots cannot be negative.");
      }

      writer.WriteByte(payload.Action);
      Write7BitEncodedInt(writer, payload.Slot);
      return;
    }

    if (payload.Slot < 0)
    {
      throw new PacketWireFormatException("Leashed entity slots cannot be negative.");
    }

    if (payload.Action is not (1 or 2)
        || payload.EntityType is not int entityType
        || entityType is < 1 or > 19)
    {
      throw new PacketWireFormatException("Leashed entity update has an unknown action or type.");
    }

    bool full = payload.Action == 1;
    if (full)
    {
      if (payload.AnchorX is not short anchorX || payload.AnchorY is not short anchorY)
      {
        throw new PacketWireFormatException("Full leashed entity updates require anchor coordinates.");
      }
    }
    else if (payload.AnchorX is not null || payload.AnchorY is not null)
    {
      throw new PacketWireFormatException("Partial leashed entity updates carry no anchor coordinates.");
    }

    writer.WriteByte(payload.Action);
    Write7BitEncodedInt(writer, payload.Slot);
    Write7BitEncodedInt(writer, entityType);
    if (full)
    {
      writer.WriteInt16(payload.AnchorX!.Value);
      writer.WriteInt16(payload.AnchorY!.Value);
    }

    if (entityType == 1 && payload.State is KiteLeashedEntityState kite)
    {
      WriteKiteState(writer, kite, full);
      return;
    }

    if (entityType is >= 2 and <= 19 && payload.State is CritterLeashedEntityState critter)
    {
      WriteCritterState(writer, entityType, critter, full);
      return;
    }

    throw new PacketWireFormatException("Leashed entity type and typed state do not match.");
  }

  private static LeashedEntityMessage ReadLeashedEntity(PacketWireReader reader)
  {
    byte action = reader.ReadByte();
    int slot = Read7BitEncodedInt(reader);
    if (slot < 0)
    {
      throw new PacketWireFormatException("Leashed entity slot cannot be negative.");
    }

    if (action == 0)
    {
      return new LeashedEntityMessage(0, slot, null, null, null, null);
    }

    if (action is not (1 or 2))
    {
      throw new PacketWireFormatException($"Unknown Version4 leashed-entity action {action}.");
    }

    int entityType = Read7BitEncodedInt(reader);
    if (entityType is < 1 or > 19)
    {
      throw new PacketWireFormatException($"Unknown Version4 leashed-entity type {entityType}.");
    }

    short? anchorX = null;
    short? anchorY = null;
    if (action == 1)
    {
      anchorX = reader.ReadInt16();
      anchorY = reader.ReadInt16();
    }

    LeashedEntityState state = entityType == 1
      ? ReadKiteState(reader, action == 1)
      : ReadCritterState(reader, entityType, action == 1);
    return new LeashedEntityMessage(action, slot, entityType, anchorX, anchorY, state);
  }

  private static void WriteKiteState(
    PacketWireWriter writer,
    KiteLeashedEntityState state,
    bool full)
  {
    if (full)
    {
      if (state.ProjectileType is not int projectileType || projectileType < 0)
      {
        throw new PacketWireFormatException("Full kite data requires a nonnegative projectile type.");
      }

      Write7BitEncodedInt(writer, projectileType);
    }
    else if (state.ProjectileType is not null)
    {
      throw new PacketWireFormatException("Partial kite data cannot include its projectile type.");
    }

    writer.WriteSingle(state.PositionX);
    writer.WriteSingle(state.PositionY);
    writer.WriteUInt32(state.PackedVelocity);
    writer.WriteByte(state.Rotation);
    writer.WriteSingle(state.WindTarget);
    writer.WriteSingle(state.CloudAlpha);
    writer.WriteSingle(state.TimeCounter);
  }

  private static KiteLeashedEntityState ReadKiteState(PacketWireReader reader, bool full)
  {
    int? projectileType = full ? Read7BitEncodedInt(reader) : null;
    return new KiteLeashedEntityState(
      projectileType,
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadUInt32(),
      reader.ReadByte(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle());
  }

  private static void WriteCritterState(
    PacketWireWriter writer,
    int entityType,
    CritterLeashedEntityState state,
    bool full)
  {
    if (full)
    {
      if (state.NpcType is not int npcType || npcType < 0
          || state.SizeX is not float sizeX || state.SizeY is not float sizeY)
      {
        throw new PacketWireFormatException("Full critter data requires its NPC type and size.");
      }

      Write7BitEncodedInt(writer, npcType);
      writer.WriteSingle(sizeX);
      writer.WriteSingle(sizeY);
    }
    else if (state.NpcType is not null || state.SizeX is not null || state.SizeY is not null)
    {
      throw new PacketWireFormatException("Partial critter data cannot include its NPC type or size.");
    }

    writer.WriteUInt32(state.PackedRelativePosition);
    writer.WriteBoolean(state.DirectionPositive);
    writer.WriteUInt32(state.RandomState);
    writer.WriteInt16(state.WaitTime);
    writer.WriteByte(state.State);
    writer.WriteSByte(state.TargetRelativeX);
    writer.WriteSByte(state.TargetRelativeY);

    if (full && entityType == 7)
    {
      if (state.ButterflyVariant is not byte variant || state.OldPositionsLength is not null)
      {
        throw new PacketWireFormatException("Full normal-butterfly data requires its variant byte.");
      }

      writer.WriteByte(variant);
    }
    else if (full && entityType == 11)
    {
      if (state.OldPositionsLength is not byte oldPositionsLength || state.ButterflyVariant is not null)
      {
        throw new PacketWireFormatException("Full shimmer-fly data requires its old-position length.");
      }

      writer.WriteByte(oldPositionsLength);
    }
    else if (state.ButterflyVariant is not null || state.OldPositionsLength is not null)
    {
      throw new PacketWireFormatException("This critter update carries no subtype-specific byte.");
    }
  }

  private static CritterLeashedEntityState ReadCritterState(
    PacketWireReader reader,
    int entityType,
    bool full)
  {
    int? npcType = full ? Read7BitEncodedInt(reader) : null;
    float? sizeX = full ? reader.ReadSingle() : null;
    float? sizeY = full ? reader.ReadSingle() : null;
    uint packedRelativePosition = reader.ReadUInt32();
    bool directionPositive = reader.ReadBoolean();
    uint randomState = reader.ReadUInt32();
    short waitTime = reader.ReadInt16();
    byte state = reader.ReadByte();
    sbyte targetRelativeX = reader.ReadSByte();
    sbyte targetRelativeY = reader.ReadSByte();
    byte? butterflyVariant = full && entityType == 7 ? reader.ReadByte() : null;
    byte? oldPositionsLength = full && entityType == 11 ? reader.ReadByte() : null;
    return new CritterLeashedEntityState(
      npcType,
      sizeX,
      sizeY,
      packedRelativePosition,
      directionPositive,
      randomState,
      waitTime,
      state,
      targetRelativeX,
      targetRelativeY,
      butterflyVariant,
      oldPositionsLength);
  }

  private static void RequireRemoveShape(LeashedEntityMessage payload)
  {
    if (payload.EntityType is not null
        || payload.AnchorX is not null
        || payload.AnchorY is not null
        || payload.State is not null)
    {
      throw new PacketWireFormatException("Leashed entity removal carries only its slot.");
    }
  }

  private static void WriteUnbreakableWall(
    PacketWireWriter writer,
    UnbreakableWallNotificationData payload)
  {
    writer.WriteByte(payload.Player);
    writer.WriteBoolean(payload.Inside);
  }

  private static UnbreakableWallNotificationData ReadUnbreakableWall(PacketWireReader reader)
  {
    return new UnbreakableWallNotificationData(reader.ReadByte(), reader.ReadBoolean());
  }

  private static void Write7BitEncodedInt(PacketWireWriter writer, int value)
  {
    if (value < 0)
    {
      throw new PacketWireFormatException("This Version4 7-bit integer must be nonnegative.");
    }

    uint remaining = (uint)value;
    while (remaining >= 0x80)
    {
      writer.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    writer.WriteByte((byte)remaining);
  }

  private static int Read7BitEncodedInt(PacketWireReader reader)
  {
    uint value = 0;
    for (int shift = 0; shift <= 28; shift += 7)
    {
      byte part = reader.ReadByte();
      if (shift == 28 && part > 7)
      {
        throw new PacketWireFormatException("Version4 7-bit integer overflows Int32.");
      }

      value |= (uint)(part & 0x7F) << shift;
      if ((part & 0x80) == 0)
      {
        return (int)value;
      }
    }

    throw new PacketWireFormatException("Version4 7-bit integer has an invalid prefix.");
  }

  private static int ReadSigned7BitEncodedInt(PacketWireReader reader)
  {
    uint value = 0;
    for (int shift = 0; shift <= 28; shift += 7)
    {
      byte part = reader.ReadByte();
      if (shift == 28 && part > 15)
      {
        throw new PacketWireFormatException("Version4 signed 7-bit integer overflows Int32.");
      }

      value |= (uint)(part & 0x7F) << shift;
      if ((part & 0x80) == 0)
      {
        return unchecked((int)value);
      }
    }

    throw new PacketWireFormatException("Version4 signed 7-bit integer has an invalid prefix.");
  }

  private static void WriteSigned7BitEncodedInt(PacketWireWriter writer, int value)
  {
    uint remaining = unchecked((uint)value);
    while (remaining >= 0x80)
    {
      writer.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    writer.WriteByte((byte)remaining);
  }

  private static T Require<T>(object? payload)
  {
    return payload is T typed
      ? typed
      : throw new PacketWireFormatException($"Packet 82 module requires {typeof(T).Name} payload.");
  }

  private sealed class TextModuleCodec(bool isServer) : IPacket82ModuleWireCodec
  {
    public ushort ModuleId => 1;

    public void Write(
      PacketWireWriter writer,
      object? payload,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      if (payload is Packet82TextData serverText)
      {
        WriteServerText(writer, serverText);
        return;
      }

      if (payload is TextCommandData clientCommand)
      {
        ArgumentNullException.ThrowIfNull(clientCommand.CommandName);
        ArgumentNullException.ThrowIfNull(clientCommand.Text);
        writer.WriteString(clientCommand.CommandName);
        writer.WriteString(clientCommand.Text);
        return;
      }

      throw new PacketWireFormatException("Packet 82 text payload must be a command or server text.");
    }

    public object Read(
      PacketWireReader reader,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      if (isServer)
      {
        return new TextCommandData(reader.ReadString(), reader.ReadString());
      }

      byte author = reader.ReadByte();
      NetworkText text = NetworkTextCodec.Read(reader);
      return new Packet82TextData(
        author,
        text,
        reader.ReadByte(),
        reader.ReadByte(),
        reader.ReadByte());
    }

    private static void WriteServerText(PacketWireWriter writer, Packet82TextData payload)
    {
      ArgumentNullException.ThrowIfNull(payload.Text);
      writer.WriteByte(payload.Author);
      NetworkTextCodec.Write(writer, payload.Text);
      writer.WriteByte(payload.Red);
      writer.WriteByte(payload.Green);
      writer.WriteByte(payload.Blue);
    }
  }

  private sealed class CraftingRequestsModuleCodec(bool isServer, ushort moduleId) : IPacket82ModuleWireCodec
  {
    public ushort ModuleId { get; } = moduleId;

    public void Write(
      PacketWireWriter writer,
      object? payload,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      if (payload is CraftingApprovalData approval)
      {
        writer.WriteBoolean(approval.Approved);
        return;
      }

      if (payload is CraftingRequestData request)
      {
        WriteCraftingRequest(writer, request);
        return;
      }

      throw new PacketWireFormatException("Packet 82 crafting payload must be a request or approval.");
    }

    public object Read(
      PacketWireReader reader,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      return isServer ? ReadCraftingRequest(reader) : new CraftingApprovalData(reader.ReadBoolean());
    }

    private static void WriteCraftingRequest(PacketWireWriter writer, CraftingRequestData request)
    {
      ArgumentNullException.ThrowIfNull(request.Materials);
      ArgumentNullException.ThrowIfNull(request.ChestIndexes);
      RequireCraftingRequestWithinFrame(request);
      Write7BitEncodedInt(writer, request.Materials.Count);
      foreach (CraftingMaterialEntry material in request.Materials)
      {
        writer.WriteInt32(material.ItemIdOrRecipeGroup);
        WriteSigned7BitEncodedInt(writer, material.Stack);
      }

      Write7BitEncodedInt(writer, request.ChestIndexes.Count);
      foreach (int chestIndex in request.ChestIndexes)
      {
        WriteSigned7BitEncodedInt(writer, chestIndex);
      }
    }

    private static CraftingRequestData ReadCraftingRequest(PacketWireReader reader)
    {
      int materialCount = Read7BitEncodedInt(reader);
      if (materialCount < 0 || reader.Remaining < 1 || materialCount > (reader.Remaining - 1) / 5)
      {
        throw new PacketWireFormatException("Packet 82 crafting material count exceeds its bounded body.");
      }

      var materials = new CraftingMaterialEntry[materialCount];
      for (int index = 0; index < materials.Length; index++)
      {
        materials[index] = new CraftingMaterialEntry(
          reader.ReadInt32(),
          ReadSigned7BitEncodedInt(reader));
      }

      int chestCount = Read7BitEncodedInt(reader);
      if (chestCount < 0 || chestCount > reader.Remaining)
      {
        throw new PacketWireFormatException("Packet 82 crafting chest count exceeds its bounded body.");
      }

      var chestIndexes = new int[chestCount];
      for (int index = 0; index < chestIndexes.Length; index++)
      {
        chestIndexes[index] = ReadSigned7BitEncodedInt(reader);
      }

      return new CraftingRequestData(materials, chestIndexes);
    }

    private static void RequireCraftingRequestWithinFrame(CraftingRequestData request)
    {
      long byteCount = GetNonnegative7BitSize(request.Materials.Count)
        + GetNonnegative7BitSize(request.ChestIndexes.Count);
      foreach (CraftingMaterialEntry material in request.Materials)
      {
        byteCount += sizeof(int);
        byteCount += GetSigned7BitSize(material.Stack);
      }

      foreach (int chestIndex in request.ChestIndexes)
      {
        byteCount += GetSigned7BitSize(chestIndex);
      }

      if (byteCount > MaximumModuleBodyBytes)
      {
        throw new PacketWireFormatException("Packet 82 crafting request exceeds the maximum frame size.");
      }
    }

    private static int GetNonnegative7BitSize(int value)
    {
      if (value < 0)
      {
        throw new PacketWireFormatException("Packet 82 collection counts cannot be negative.");
      }

      return GetSigned7BitSize(value);
    }

    private static int GetSigned7BitSize(int value)
    {
      uint remaining = unchecked((uint)value);
      int size = 1;
      while (remaining >= 0x80)
      {
        size++;
        remaining >>= 7;
      }

      return size;
    }
  }

  private sealed class UnbreakableWallModuleCodec(bool isServer) : IPacket82ModuleWireCodec
  {
    public ushort ModuleId => 14;

    public void Write(
      PacketWireWriter writer,
      object? payload,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      if (!isServer)
      {
        throw new PacketWireFormatException(
          "Packet 82 unbreakable-wall notifications are server-to-client only.");
      }

      WriteUnbreakableWall(writer, Require<UnbreakableWallNotificationData>(payload));
    }

    public object Read(
      PacketWireReader reader,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      if (isServer)
      {
        throw new PacketWireFormatException(
          "Packet 82 unbreakable-wall notifications have no server inbound layout.");
      }

      return ReadUnbreakableWall(reader);
    }
  }

  private sealed class ContextModuleCodec<TPayload>(
    ushort moduleId,
    Action<PacketWireWriter, TPayload, int, Func<short, bool>> write,
    Func<PacketWireReader, int, Func<short, bool>, TPayload> read) : IPacket82ModuleWireCodec
    where TPayload : notnull
  {
    public ushort ModuleId { get; } = moduleId;

    public void Write(
      PacketWireWriter writer,
      object? payload,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      write(writer, Require<TPayload>(payload), tagEffectNpcSlotCount, tagEffectUsesProcTimes);
    }

    public object Read(
      PacketWireReader reader,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      return read(reader, tagEffectNpcSlotCount, tagEffectUsesProcTimes);
    }
  }

  private sealed class ModuleCodec<TPayload>(
    ushort moduleId,
    Action<PacketWireWriter, TPayload> write,
    Func<PacketWireReader, TPayload> read) : IPacket82ModuleWireCodec
    where TPayload : notnull
  {
    public ushort ModuleId { get; } = moduleId;

    public void Write(
      PacketWireWriter writer,
      object? payload,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      write(writer, Require<TPayload>(payload));
    }

    public object Read(
      PacketWireReader reader,
      int tagEffectNpcSlotCount,
      Func<short, bool> tagEffectUsesProcTimes)
    {
      return read(reader);
    }
  }
}
