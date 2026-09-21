using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Protocol.V1456.Packets;

public static class TerrariaPacketCodec
{
  private const byte PlayerControlsMountBit = 1 << 7;
  private const byte ChestTransferRevisionExtensionMarker = 0xD1;
  private const byte ControlDownBit = 1 << 1;
  private const byte ControlLeftBit = 1 << 2;
  private const byte ControlRightBit = 1 << 3;
  private const byte ControlJumpBit = 1 << 4;
  private const byte ControlUseItemBit = 1 << 5;
  private const byte FacingRightBit = 1 << 6;
  private const int RequiredTileManipulationLength = 8;
  private const int RequiredChestOpenLength = 9;
  private const int RequiredDoorToggleLength = 6;
  private const int RequiredEquipmentLength = 9;
  private const int RequiredLoadoutLength = 4;
  private const int RequiredPlayerZoneLength = 7;
  private const int RequiredVitalsLength = 5;
  private const int RequiredClientTalkNpcLength = 3;
  private const int RequiredClientProjectileTerminationLength = 3;
  private const int RequiredAddPlayerBuffPvpLength = sizeof(byte) + sizeof(ushort) + sizeof(int);
  private const int MinimumClientProjectileLength = 22;
  private const ushort CreativePowerPermissionsModuleId = 9;
  private const ushort CreativeUnlocksPlayerReportModuleId = 6;
  private const ushort BestiaryModuleId = 4;
  private const ushort BannerModuleId = 10;
  private const ushort CraftingRequestsModuleId = 11;
  private const ushort TagEffectModuleId = 12;
  private const ushort LeashedEntityModuleId = 13;
  private const ushort TextModuleId = 1;
  private const ushort ParticleOrchestraModuleId = 8;
  private const ushort TeleportPylonModuleId = 7;
  private const byte SetCreativePowerPermissionLevelSelector = 0;
  private const byte MaximumTeleportPylonSubPacketType = 2;
  private const ushort JourneySpawnRateModuleId = 5;
  private const ushort JourneySpawnRatePowerId = 14;
  private const int PerPlayerToggleStateByteCount = 32;
  private const int DefaultBannerTypeCount = 293;
  private const int MaximumTagEffectNpcCount = 200;
  private const int MaximumLeashedEntityType = 19;
  private const int PlayerItemSlotCount = 990;
  private const int MaximumBuffCount = 44;
  private const int MaximumUuidLength = 64;
  private const short SectionHeight = 150;
  private const short SectionWidth = 200;
  private const int InitialSectionColumns = 5;
  private const int InitialSectionRows = 3;
  private const int SpawnSectionX = 10;
  private const int SpawnSectionY = 2;

  public static PlayerControlIntent DecodePlayerControls(ReadOnlySpan<byte> frameBytes)
  {
    return DecodePlayerControlsCompatibility(frameBytes).Intent;
  }

  public static LegacyPlayerControlsProjection DecodePlayerControlsCompatibility(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerControls)
    {
      throw new InvalidDataException("Terraria frame is not a PlayerControls packet.");
    }

    return TerrariaV1456Compatibility.DecodePlayerControls(frame.Payload.Span);
  }

  public static JourneySpawnRatePacket DecodeJourneySpawnRate(ReadOnlySpan<byte> frameBytes)
  {
    CreativePowerModulePacket packet = DecodeCreativePowerModule(frameBytes);
    if (packet.PowerId != JourneySpawnRatePowerId ||
        packet.PayloadKind != CreativePowerPayloadKind.PerPlayerSlider ||
        packet.PlayerSlot is not byte playerSlot ||
        packet.SliderValue is not float sliderValue)
    {
      throw new InvalidDataException("Terraria frame is not a valid Journey spawn-rate NetModule.");
    }

    if (!float.IsFinite(sliderValue) || sliderValue < 0.0f || sliderValue > 1.0f)
    {
      throw new InvalidDataException("Terraria Journey spawn-rate value is outside its valid range.");
    }

    return new JourneySpawnRatePacket(playerSlot, sliderValue);
  }

  public static CreativePowerModulePacket DecodeCreativePowerModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != JourneySpawnRateModuleId || packet.Payload.Length < sizeof(ushort))
    {
      throw new InvalidDataException("Terraria frame is not a CreativePowers NetModule.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    ushort powerId = reader.ReadUInt16();
    CreativePowerPayloadKind payloadKind = GetCreativePowerPayloadKind(powerId);
    switch (payloadKind)
    {
      case CreativePowerPayloadKind.SharedButton:
        RequireCreativePowerPayloadEnd(stream, powerId);
        return new CreativePowerModulePacket(
          powerId,
          payloadKind,
          PlayerSlot: null,
          ToggleState: null,
          SliderValue: null,
          PlayerStateBits: null);
      case CreativePowerPayloadKind.SharedToggle:
      {
        RequireCreativePowerPayloadAtLeast(stream, powerId, sizeof(byte));
        bool toggleState = reader.ReadBoolean();
        RequireCreativePowerPayloadEnd(stream, powerId);
        return new CreativePowerModulePacket(
          powerId,
          payloadKind,
          PlayerSlot: null,
          toggleState,
          SliderValue: null,
          PlayerStateBits: null);
      }
      case CreativePowerPayloadKind.SharedSlider:
      {
        RequireCreativePowerPayloadAtLeast(stream, powerId, sizeof(float));
        float sliderValue = reader.ReadSingle();
        RequireCreativePowerPayloadEnd(stream, powerId);
        return new CreativePowerModulePacket(
          powerId,
          payloadKind,
          PlayerSlot: null,
          ToggleState: null,
          sliderValue,
          PlayerStateBits: null);
      }
      case CreativePowerPayloadKind.PerPlayerToggle:
      {
        RequireCreativePowerPayloadAtLeast(stream, powerId, sizeof(byte));
        byte subMessageType = reader.ReadByte();
        if (subMessageType == 0)
        {
          RequireCreativePowerPayloadLength(stream, powerId, PerPlayerToggleStateByteCount);
          byte[] stateBits = reader.ReadBytes(PerPlayerToggleStateByteCount);
          if (stateBits.Length != PerPlayerToggleStateByteCount)
          {
            throw new InvalidDataException(
              $"Terraria CreativePower {powerId} has a truncated per-player state bitset.");
          }

          RequireCreativePowerPayloadEnd(stream, powerId);
          return new CreativePowerModulePacket(
            powerId,
            CreativePowerPayloadKind.PerPlayerToggleSyncEveryone,
            PlayerSlot: null,
            ToggleState: null,
            SliderValue: null,
            stateBits);
        }

        if (subMessageType == 1)
        {
          RequireCreativePowerPayloadLength(stream, powerId, sizeof(byte) + sizeof(byte));
          byte playerSlot = reader.ReadByte();
          bool toggleState = reader.ReadBoolean();
          RequireCreativePowerPayloadEnd(stream, powerId);
          return new CreativePowerModulePacket(
            powerId,
            CreativePowerPayloadKind.PerPlayerToggleSyncOnePlayer,
            playerSlot,
            toggleState,
            SliderValue: null,
            PlayerStateBits: null);
        }

        throw new InvalidDataException(
          $"Terraria CreativePower {powerId} has an unsupported per-player toggle subtype.");
      }
      case CreativePowerPayloadKind.PerPlayerSlider:
      {
        RequireCreativePowerPayloadAtLeast(stream, powerId, sizeof(byte) + sizeof(float));
        byte playerSlot = reader.ReadByte();
        float sliderValue = reader.ReadSingle();
        RequireCreativePowerPayloadEnd(stream, powerId);
        return new CreativePowerModulePacket(
          powerId,
          payloadKind,
          playerSlot,
          ToggleState: null,
          sliderValue,
          PlayerStateBits: null);
      }
      default:
        throw new InvalidDataException("Terraria CreativePower payload kind is unsupported.");
    }
  }

  public static NetModulePacket DecodeNetModule(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.NetModules || frame.Payload.Length < sizeof(ushort))
    {
      throw new InvalidDataException("Terraria NetModules packet must include a module identifier.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    ushort moduleId = BinaryPrimitives.ReadUInt16LittleEndian(payload);
    return new NetModulePacket(moduleId, payload[sizeof(ushort)..].ToArray());
  }

  public static ChatMessageModulePacket DecodeClientTextModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != TextModuleId)
    {
      throw new InvalidDataException("Terraria NetModules frame is not a client Text module.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    string commandId = reader.ReadString();
    string text = reader.ReadString();
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria client Text module contains trailing payload bytes.");
    }

    return new ChatMessageModulePacket(commandId, text);
  }

  public static ServerTextModulePacket DecodeServerTextModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != TextModuleId || packet.Payload.Length < sizeof(byte))
    {
      throw new InvalidDataException("Terraria NetModules frame is not a server Text module.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    byte authorId = reader.ReadByte();
    LegacyNetworkText text = ReadNetworkText(reader);
    TerrariaColor color = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria server Text module contains trailing payload bytes.");
    }

    return new ServerTextModulePacket(authorId, text, color);
  }

  public static NetPingModulePacket DecodeNetPingModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != 2 || packet.Payload.Length != sizeof(float) * 2)
    {
      throw new InvalidDataException("Terraria NetModules Ping must contain one Vector2 payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    return new NetPingModulePacket(
      BinaryPrimitives.ReadSingleLittleEndian(payload),
      BinaryPrimitives.ReadSingleLittleEndian(payload[sizeof(float)..]));
  }

  public static IReadOnlyList<WorldLiquidSnapshot> DecodeLiquidNetModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != 0 || packet.Payload.Length < sizeof(ushort))
    {
      throw new InvalidDataException("Terraria NetModules Liquid must include an update count.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    int count = BinaryPrimitives.ReadUInt16LittleEndian(payload);
    const int UpdateLength = sizeof(int) + sizeof(byte) + sizeof(byte);
    if (payload.Length != sizeof(ushort) + count * UpdateLength)
    {
      throw new InvalidDataException("Terraria NetModules Liquid length does not match its update count.");
    }

    List<WorldLiquidSnapshot> changes = new(count);
    int offset = sizeof(ushort);
    for (int index = 0; index < count; index++)
    {
      int packedCoordinates = BinaryPrimitives.ReadInt32LittleEndian(payload[offset..]);
      int x = (packedCoordinates >> 16) & ushort.MaxValue;
      int y = packedCoordinates & ushort.MaxValue;
      changes.Add(new WorldLiquidSnapshot(x, y, payload[offset + sizeof(int)],
        payload[offset + sizeof(int) + sizeof(byte)]));
      offset += UpdateLength;
    }

    return changes;
  }

  public static UnbreakableWallScanModulePacket DecodeUnbreakableWallScanModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != 14 || packet.Payload.Length != sizeof(byte) * 2)
    {
      throw new InvalidDataException(
        "Terraria NetModules wall scan must contain a player slot and state flag.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    return new UnbreakableWallScanModulePacket(payload[0], payload[1] != 0);
  }

  public static NetAmbienceModulePacket DecodeNetAmbienceModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int AmbiencePayloadLength = sizeof(byte) + sizeof(int) + sizeof(byte);
    if (packet.ModuleId != 3 || packet.Payload.Length != AmbiencePayloadLength)
    {
      throw new InvalidDataException("Terraria NetModules ambience must contain its fixed payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    return new NetAmbienceModulePacket(
      payload[0],
      BinaryPrimitives.ReadInt32LittleEndian(payload[sizeof(byte)..]),
      payload[sizeof(byte) + sizeof(int)]);
  }

  public static BestiaryModulePacket DecodeBestiaryModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int MinimumBestiaryPayloadLength = sizeof(byte) + sizeof(short);
    if (packet.ModuleId != BestiaryModuleId ||
        packet.Payload.Length < MinimumBestiaryPayloadLength)
    {
      throw new InvalidDataException(
        "Terraria NetModules Bestiary must contain its type and NPC net ID.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    BestiaryUnlockType unlockType = (BestiaryUnlockType)reader.ReadByte();
    short npcNetId = reader.ReadInt16();
    switch (unlockType)
    {
      case BestiaryUnlockType.Kill:
      {
        int killCount = reader.Read7BitEncodedInt();
        if (stream.Position != stream.Length)
        {
          throw new InvalidDataException(
            "Terraria NetModules Bestiary Kill contains trailing payload bytes.");
        }

        return new BestiaryModulePacket(unlockType, npcNetId, killCount);
      }
      case BestiaryUnlockType.Sight:
      case BestiaryUnlockType.Chat:
        if (stream.Position != stream.Length)
        {
          throw new InvalidDataException(
            "Terraria NetModules Bestiary contains trailing payload bytes.");
        }

        return new BestiaryModulePacket(unlockType, npcNetId, null);
      default:
        throw new InvalidDataException("Terraria NetModules Bestiary type is unsupported.");
    }
  }

  public static BannerModulePacket DecodeBannerModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != BannerModuleId || packet.Payload.Length < sizeof(byte))
    {
      throw new InvalidDataException("Terraria NetModules frame is not a Banners module.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    BannerModuleMessageType messageType = (BannerModuleMessageType)reader.ReadByte();
    switch (messageType)
    {
      case BannerModuleMessageType.FullState:
      {
        int[] killCounts = ReadBannerKillCounts(reader, stream);
        ushort[] claimableCounts = ReadBannerClaimableCounts(reader, stream);
        RequireBannerPayloadEnd(stream);
        return new BannerModulePacket(
          messageType,
          BannerId: null,
          KillCount: null,
          ClaimCount: null,
          Granted: null,
          killCounts,
          claimableCounts);
      }
      case BannerModuleMessageType.KillCountUpdate:
      {
        RequireBannerPayloadLength(stream, sizeof(short) + sizeof(int));
        short bannerId = reader.ReadInt16();
        int killCount = reader.ReadInt32();
        return new BannerModulePacket(
          messageType,
          bannerId,
          killCount,
          ClaimCount: null,
          Granted: null,
          KillCounts: null,
          ClaimableCounts: null);
      }
      case BannerModuleMessageType.ClaimCountUpdate:
      case BannerModuleMessageType.ClaimRequest:
      {
        RequireBannerPayloadLength(stream, sizeof(short) + sizeof(ushort));
        short bannerId = reader.ReadInt16();
        ushort claimCount = reader.ReadUInt16();
        return new BannerModulePacket(
          messageType,
          bannerId,
          KillCount: null,
          claimCount,
          Granted: null,
          KillCounts: null,
          ClaimableCounts: null);
      }
      case BannerModuleMessageType.ClaimResponse:
      {
        RequireBannerPayloadLength(stream, sizeof(short) + sizeof(ushort) + sizeof(byte));
        short bannerId = reader.ReadInt16();
        ushort claimCount = reader.ReadUInt16();
        bool granted = reader.ReadBoolean();
        return new BannerModulePacket(
          messageType,
          bannerId,
          KillCount: null,
          claimCount,
          granted,
          KillCounts: null,
          ClaimableCounts: null);
      }
      default:
        throw new InvalidDataException("Terraria Banners module message type is unsupported.");
    }
  }

  public static CraftingRequestModulePacket DecodeCraftingRequestModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != CraftingRequestsModuleId)
    {
      throw new InvalidDataException("Terraria NetModules frame is not a crafting request.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    int itemCount = ReadCraftingCount(reader, stream, "item");
    List<CraftingRequiredItemPacket> items = new(itemCount);
    for (int index = 0; index < itemCount; index++)
    {
      RequireCraftingPayloadAtLeast(stream, sizeof(int));
      items.Add(new CraftingRequiredItemPacket(
        reader.ReadInt32(),
        ReadCraftingInt(reader, stream, "item stack")));
    }

    int chestCount = ReadCraftingCount(reader, stream, "chest");
    List<int> chestIndices = new(chestCount);
    for (int index = 0; index < chestCount; index++)
    {
      chestIndices.Add(ReadCraftingInt(reader, stream, "chest index"));
    }

    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException(
        "Terraria crafting request contains trailing payload bytes.");
    }

    return new CraftingRequestModulePacket(items, chestIndices);
  }

  public static CraftingResponseModulePacket DecodeCraftingResponseModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != CraftingRequestsModuleId || packet.Payload.Length != sizeof(byte))
    {
      throw new InvalidDataException("Terraria NetModules frame is not a crafting response.");
    }

    return new CraftingResponseModulePacket(packet.Payload.Span[0] != 0);
  }

  public static TagEffectModulePacket DecodeTagEffectModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != TagEffectModuleId || packet.Payload.Length < sizeof(byte) * 2)
    {
      throw new InvalidDataException("Terraria NetModules frame is not a tag-effect module.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    byte ownerSlot = reader.ReadByte();
    TagEffectMessageType messageType = (TagEffectMessageType)reader.ReadByte();
    switch (messageType)
    {
      case TagEffectMessageType.FullState:
      {
        RequireTagEffectPayloadAtLeast(stream, sizeof(short));
        short effectType = reader.ReadInt16();
        IReadOnlyList<TagEffectSparseEntry> taggedNpcTimes = ReadTagEffectSparseArray(
          reader,
          stream);
        RequireTagEffectPayloadEnd(stream);
        return new TagEffectModulePacket(
          ownerSlot,
          messageType,
          effectType,
          taggedNpcTimes,
          ProcNpcTimes: null,
          NpcIndex: null);
      }
      case TagEffectMessageType.ChangeActiveEffect:
      {
        RequireTagEffectPayloadLength(stream, sizeof(short));
        return new TagEffectModulePacket(
          ownerSlot,
          messageType,
          reader.ReadInt16(),
          TaggedNpcTimes: null,
          ProcNpcTimes: null,
          NpcIndex: null);
      }
      case TagEffectMessageType.ApplyTagToNpc:
      case TagEffectMessageType.EnableProcOnNpc:
      case TagEffectMessageType.ClearProcOnNpc:
      {
        RequireTagEffectPayloadLength(stream, sizeof(byte));
        return new TagEffectModulePacket(
          ownerSlot,
          messageType,
          EffectType: null,
          TaggedNpcTimes: null,
          ProcNpcTimes: null,
          reader.ReadByte());
      }
      default:
        throw new InvalidDataException("Terraria tag-effect message type is unsupported.");
    }
  }

  public static LeashedEntityModulePacket DecodeLeashedEntityModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    if (packet.ModuleId != LeashedEntityModuleId || packet.Payload.Length < sizeof(byte) * 2)
    {
      throw new InvalidDataException("Terraria NetModules frame is not a leashed-entity module.");
    }

    using MemoryStream stream = new(packet.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    LeashedEntityMessageType messageType = (LeashedEntityMessageType)reader.ReadByte();
    int slot = ReadLeashedEntityInt(reader, stream, "slot");
    if (messageType == LeashedEntityMessageType.Remove)
    {
      RequireLeashedEntityPayloadEnd(stream);
      return new LeashedEntityModulePacket(
        messageType,
        slot,
        EntityType: null,
        AnchorX: null,
        AnchorY: null,
        KiteState: null,
        CritterState: null);
    }

    if (messageType != LeashedEntityMessageType.FullSync &&
        messageType != LeashedEntityMessageType.PartialSync)
    {
      throw new InvalidDataException("Terraria leashed-entity message type is unsupported.");
    }

    int entityType = ReadLeashedEntityInt(reader, stream, "type");
    if (entityType < 1 || entityType > MaximumLeashedEntityType)
    {
      throw new InvalidDataException("Terraria leashed-entity type is not registered in V1456.");
    }

    bool full = messageType == LeashedEntityMessageType.FullSync;
    short? anchorX = null;
    short? anchorY = null;
    if (full)
    {
      RequireLeashedEntityPayloadAtLeast(stream, sizeof(short) * 2);
      anchorX = reader.ReadInt16();
      anchorY = reader.ReadInt16();
    }

    if (entityType == 1)
    {
      LeashedKiteStatePacket kiteState = ReadLeashedKiteState(reader, stream, full);
      RequireLeashedEntityPayloadEnd(stream);
      return new LeashedEntityModulePacket(
        messageType,
        slot,
        entityType,
        anchorX,
        anchorY,
        kiteState,
        CritterState: null);
    }

    LeashedCritterStatePacket critterState = ReadLeashedCritterState(
      reader,
      stream,
      entityType,
      full);
    RequireLeashedEntityPayloadEnd(stream);
    return new LeashedEntityModulePacket(
      messageType,
      slot,
      entityType,
      anchorX,
      anchorY,
      KiteState: null,
      critterState);
  }

  public static CreativePowerPermissionModulePacket DecodeCreativePowerPermissionModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int PermissionPayloadLength = sizeof(byte) + sizeof(ushort) + sizeof(byte);
    if (packet.ModuleId != CreativePowerPermissionsModuleId ||
        packet.Payload.Length != PermissionPayloadLength)
    {
      throw new InvalidDataException(
        "Terraria NetModules power permission must contain its selector and fixed payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    if (payload[0] != SetCreativePowerPermissionLevelSelector)
    {
      throw new InvalidDataException(
        "Terraria NetModules power permission selector is unsupported.");
    }

    return new CreativePowerPermissionModulePacket(
      BinaryPrimitives.ReadUInt16LittleEndian(payload[sizeof(byte)..]),
      payload[sizeof(byte) + sizeof(ushort)]);
  }

  public static CreativeUnlocksPlayerReportModulePacket
    DecodeCreativeUnlocksPlayerReportModule(ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int UnlockReportPayloadLength =
      sizeof(byte) + sizeof(ushort) + sizeof(ushort);
    if (packet.ModuleId != CreativeUnlocksPlayerReportModuleId ||
        packet.Payload.Length != UnlockReportPayloadLength)
    {
      throw new InvalidDataException(
        "Terraria NetModules creative unlock report must contain its fixed payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    return new CreativeUnlocksPlayerReportModulePacket(
      payload[0],
      BinaryPrimitives.ReadUInt16LittleEndian(payload[sizeof(byte)..]),
      BinaryPrimitives.ReadUInt16LittleEndian(
        payload[(sizeof(byte) + sizeof(ushort))..]));
  }

  public static TeleportPylonModulePacket DecodeTeleportPylonModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int TeleportPylonPayloadLength =
      sizeof(byte) + sizeof(short) + sizeof(short) + sizeof(byte);
    if (packet.ModuleId != TeleportPylonModuleId ||
        packet.Payload.Length != TeleportPylonPayloadLength)
    {
      throw new InvalidDataException("Terraria NetModules pylon must contain its fixed payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    if (payload[0] > MaximumTeleportPylonSubPacketType)
    {
      throw new InvalidDataException("Terraria NetModules pylon selector is unsupported.");
    }

    return new TeleportPylonModulePacket(
      payload[0],
      BinaryPrimitives.ReadInt16LittleEndian(payload[sizeof(byte)..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[(sizeof(byte) + sizeof(short))..]),
      payload[sizeof(byte) + sizeof(short) + sizeof(short)]);
  }

  public static ParticleOrchestraModulePacket DecodeParticleOrchestraModule(
    ReadOnlySpan<byte> frameBytes)
  {
    NetModulePacket packet = DecodeNetModule(frameBytes);
    const int ParticlePayloadLength =
      sizeof(byte) + sizeof(float) * 4 + sizeof(int) + sizeof(byte);
    if (packet.ModuleId != ParticleOrchestraModuleId ||
        packet.Payload.Length != ParticlePayloadLength)
    {
      throw new InvalidDataException(
        "Terraria NetModules particles must contain its fixed payload.");
    }

    ReadOnlySpan<byte> payload = packet.Payload.Span;
    int positionOffset = sizeof(byte);
    int movementOffset = positionOffset + sizeof(float) * 2;
    int uniqueInfoOffset = movementOffset + sizeof(float) * 2;
    int invokingPlayerOffset = uniqueInfoOffset + sizeof(int);
    return new ParticleOrchestraModulePacket(
      payload[0],
      new SimulationVector(
        BinaryPrimitives.ReadSingleLittleEndian(payload[positionOffset..]),
        BinaryPrimitives.ReadSingleLittleEndian(payload[(positionOffset + sizeof(float))..])),
      new SimulationVector(
        BinaryPrimitives.ReadSingleLittleEndian(payload[movementOffset..]),
        BinaryPrimitives.ReadSingleLittleEndian(payload[(movementOffset + sizeof(float))..])),
      BinaryPrimitives.ReadInt32LittleEndian(payload[uniqueInfoOffset..]),
      payload[invokingPlayerOffset]);
  }

  public static byte DecodeClientProjectileOwner(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncProjectile ||
        frame.Payload.Length < MinimumClientProjectileLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid client SyncProjectile packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    _ = reader.ReadInt16();
    _ = reader.ReadSingle();
    _ = reader.ReadSingle();
    _ = reader.ReadSingle();
    _ = reader.ReadSingle();
    byte playerSlot = reader.ReadByte();
    _ = reader.ReadInt16();
    byte flags = reader.ReadByte();
    byte extendedFlags = (flags & (1 << 2)) != 0 ? reader.ReadByte() : (byte)0;
    if ((flags & (1 << 0)) != 0)
    {
      _ = reader.ReadSingle();
    }

    if ((flags & (1 << 1)) != 0)
    {
      _ = reader.ReadSingle();
    }

    if ((flags & (1 << 3)) != 0)
    {
      _ = reader.ReadUInt16();
    }

    if ((flags & (1 << 4)) != 0)
    {
      _ = reader.ReadInt16();
    }

    if ((flags & (1 << 5)) != 0)
    {
      _ = reader.ReadSingle();
    }

    if ((flags & (1 << 6)) != 0)
    {
      _ = reader.ReadInt16();
    }

    if ((flags & (1 << 7)) != 0)
    {
      _ = reader.ReadInt16();
    }

    if ((extendedFlags & (1 << 0)) != 0)
    {
      _ = reader.ReadSingle();
    }

    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria client SyncProjectile has unsupported trailing data.");
    }

    return playerSlot;
  }

  public static ProjectileSyncPacket DecodeProjectileSync(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncProjectile || frame.Payload.Length < 22)
    {
      throw new InvalidDataException("Terraria frame is not a valid SyncProjectile packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    short identity = reader.ReadInt16();
    SimulationVector position = new(
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()),
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()));
    SimulationVector velocity = new(
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()),
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()));
    byte owner = reader.ReadByte();
    short projectileType = reader.ReadInt16();
    byte flags = reader.ReadByte();
    byte extendedFlags = (flags & (1 << 2)) != 0 ? reader.ReadByte() : (byte)0;
    float? ai0 = (flags & (1 << 0)) != 0 ? reader.ReadSingle() : null;
    float? ai1 = (flags & (1 << 1)) != 0 ? reader.ReadSingle() : null;
    ushort? banner = (flags & (1 << 3)) != 0 ? reader.ReadUInt16() : null;
    short? damage = (flags & (1 << 4)) != 0 ? reader.ReadInt16() : null;
    float? knockback = (flags & (1 << 5)) != 0 ? reader.ReadSingle() : null;
    short? originalDamage = (flags & (1 << 6)) != 0 ? reader.ReadInt16() : null;
    short? uuid = (flags & (1 << 7)) != 0 ? reader.ReadInt16() : null;
    float? ai2 = (extendedFlags & 1) != 0 ? reader.ReadSingle() : null;
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("SyncProjectile contains unsupported trailing data.");
    }

    return new ProjectileSyncPacket(
      identity,
      position,
      velocity,
      owner,
      projectileType,
      ai0,
      ai1,
      ai2,
      banner,
      damage,
      knockback,
      originalDamage,
      uuid);
  }

  public static ClientProjectileTermination DecodeClientProjectileTermination(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.KillProjectile ||
        frame.Payload.Length != RequiredClientProjectileTerminationLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid client KillProjectile packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new ClientProjectileTermination(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      payload[sizeof(short)]);
  }

  public static PlayerZonePacket DecodePlayerZone(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncPlayerZone ||
        frame.Payload.Length != RequiredPlayerZoneLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid SyncPlayerZone packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new PlayerZonePacket(
      payload[0],
      payload[1],
      payload[2],
      payload[3],
      payload[4],
      payload[5],
      payload[6]);
  }

  public static TileManipulationIntent DecodeTileManipulation(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TileManipulation ||
        frame.Payload.Length != RequiredTileManipulationLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid TileManipulation packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    byte actionValue = reader.ReadByte();
    if (actionValue > (byte)TileManipulationAction.PlaceTile)
    {
      throw new InvalidDataException("Terraria TileManipulation action is not supported.");
    }

    return new TileManipulationIntent(
      (TileManipulationAction)actionValue,
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadByte());
  }

  public static TileEntityPlacementIntent DecodeTileEntityPlacement(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TileEntityPlacement ||
        frame.Payload.Length != 5)
    {
      throw new InvalidDataException("Terraria frame is not a valid TileEntityPlacement packet.");
    }

    short tileX = BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span);
    short tileY = BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[2..]);
    return new TileEntityPlacementIntent(tileX, tileY, frame.Payload.Span[4]);
  }

  public static byte[] EncodeObjectPlacement(ObjectPlacementPacket packet)
  {
    if (!packet.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
      writer.Write(packet.ObjectType);
      writer.Write(packet.Style);
      writer.Write(packet.Alternate);
      writer.Write(packet.Random);
      writer.Write(packet.DirectionRight);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ObjectPlacement,
      payload.ToArray()));
  }

  public static ObjectPlacementPacket DecodeObjectPlacement(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(short) * 4 + sizeof(byte) + sizeof(sbyte) + sizeof(bool);
    if (frame.MessageId != TerrariaMessageId.ObjectPlacement ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException("Terraria ObjectPlacement packet has an invalid payload.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    ObjectPlacementPacket packet = new(
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadByte(),
      reader.ReadSByte(),
      reader.ReadBoolean());
    if (!packet.IsValid)
    {
      throw new InvalidDataException("Terraria ObjectPlacement packet contains invalid values.");
    }

    return packet;
  }

  public static ChestOpenIntent DecodeChestOpen(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RequestChestOpen ||
        frame.Payload.Length != RequiredChestOpenLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid RequestChestOpen packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new ChestOpenIntent(
      reader.ReadByte(), reader.ReadInt32(), reader.ReadInt16(), reader.ReadInt16());
  }

  public static byte[] EncodeChestOpen(ChestOpenIntent packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write(packet.ChestId);
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.RequestChestOpen,
      payload.ToArray()));
  }

  public static byte[] EncodeChestName(ChestNamePacket packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.ChestId);
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
      writer.Write(packet.Name);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ChestName,
      payload.ToArray()));
  }

  public static ChestNamePacket DecodeChestName(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.ChestName || frame.Payload.Length < 7)
    {
      throw new InvalidDataException("Terraria ChestName packet is truncated.");
    }

    try
    {
      using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
      using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
      ChestNamePacket packet = new(
        reader.ReadInt16(),
        reader.ReadInt16(),
        reader.ReadInt16(),
        reader.ReadString());
      if (stream.Position != stream.Length)
      {
        throw new InvalidDataException("Terraria ChestName packet contains trailing bytes.");
      }

      return packet;
    }
    catch (EndOfStreamException exception)
    {
      throw new InvalidDataException("Terraria ChestName packet has a truncated name.", exception);
    }
  }

  public static byte[] EncodeTravelMerchantItems(TravelMerchantItemsPacket packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      for (int index = 0; index < TravelMerchantItemsPacket.SlotCount; index++)
      {
        writer.Write(packet.ItemIds[index]);
      }
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TravelMerchantItems,
      payload.ToArray()));
  }

  public static TravelMerchantItemsPacket DecodeTravelMerchantItems(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    int expectedLength = TravelMerchantItemsPacket.SlotCount * sizeof(short);
    if (frame.MessageId != TerrariaMessageId.TravelMerchantItems ||
        frame.Payload.Length != expectedLength)
    {
      throw new InvalidDataException("Terraria TravelMerchantItems packet must contain 40 Int16 slots.");
    }

    short[] itemIds = new short[TravelMerchantItemsPacket.SlotCount];
    for (int index = 0; index < itemIds.Length; index++)
    {
      itemIds[index] = BinaryPrimitives.ReadInt16LittleEndian(
        frame.Payload.Span.Slice(index * sizeof(short), sizeof(short)));
    }

    return new TravelMerchantItemsPacket(itemIds);
  }

  public static byte[] EncodeBugCatching(BugCatchingPacket packet)
  {
    byte[] payload = new byte[sizeof(short) + sizeof(byte)];
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(0, sizeof(short)), packet.NpcId);
    payload[sizeof(short)] = packet.ReportedPlayerSlot;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.BugCatching, payload));
  }

  public static BugCatchingPacket DecodeBugCatching(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.BugCatching || frame.Payload.Length != 3)
    {
      throw new InvalidDataException("Terraria BugCatching packet must contain a three-byte payload.");
    }

    return new BugCatchingPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[..sizeof(short)]),
      frame.Payload.Span[sizeof(short)]);
  }

  public static byte[] EncodeBugReleasing(BugReleasingPacket packet)
  {
    byte[] payload = new byte[sizeof(int) + sizeof(int) + sizeof(short) + sizeof(byte)];
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0, sizeof(int)), packet.TileX);
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(int), sizeof(int)), packet.TileY);
    BinaryPrimitives.WriteInt16LittleEndian(
      payload.AsSpan(sizeof(int) + sizeof(int), sizeof(short)),
      packet.NpcType);
    payload[sizeof(int) + sizeof(int) + sizeof(short)] = packet.Style;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.BugReleasing, payload));
  }

  public static BugReleasingPacket DecodeBugReleasing(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(int) + sizeof(int) + sizeof(short) + sizeof(byte);
    if (frame.MessageId != TerrariaMessageId.BugReleasing ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException("Terraria BugReleasing packet must contain an eleven-byte payload.");
    }

    return new BugReleasingPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span[..sizeof(int)]),
      BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span.Slice(sizeof(int), sizeof(int))),
      BinaryPrimitives.ReadInt16LittleEndian(
        frame.Payload.Span.Slice(sizeof(int) + sizeof(int), sizeof(short))),
      frame.Payload.Span[sizeof(int) + sizeof(int) + sizeof(short)]);
  }

  public static byte[] EncodeChestItem(ChestItemReplicationSnapshot snapshot)
  {
    if (snapshot.ChestId <= 0 || snapshot.ChestId > short.MaxValue || snapshot.Slot >= 40 ||
        snapshot.Stack.Quantity < 0 || snapshot.Stack.Quantity > short.MaxValue ||
        snapshot.Stack.IsEmpty && snapshot.Stack != ItemStack.Empty ||
        snapshot.Stack.ItemType > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.ChestId);
      writer.Write(snapshot.Slot);
      writer.Write((short)snapshot.Stack.Quantity);
      writer.Write((byte)0);
      writer.Write((short)snapshot.Stack.ItemType);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncChestItem,
      payload.ToArray()));
  }

  public static byte[] EncodeChestSize(int chestId, int slotCount)
  {
    if (chestId <= 0 || chestId > short.MaxValue || slotCount <= 0 ||
        slotCount > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(chestId));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)chestId);
      writer.Write((short)slotCount);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncChestSize,
      payload.ToArray()));
  }

  public static ChestTransferIntent DecodeChestTransfer(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncPlayerChest ||
        (frame.Payload.Length != 8 && frame.Payload.Length != 17))
    {
      throw new InvalidDataException("Terraria frame is not a valid SyncPlayerChest packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    ChestTransferIntent intent = new(
      reader.ReadByte(),
      reader.ReadInt32(),
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadBoolean(),
      frame.Payload.Length == 17
        ? ReadExplicitChestTransferRevision(reader)
        : -1);
    if (intent.InventorySlot >= 10 || intent.ChestSlot >= 40 || intent.ExpectedRevision < -1)
    {
      throw new InvalidDataException("Terraria SyncPlayerChest references an invalid slot.");
    }

    return intent;
  }

  public static byte[] EncodeChestTransfer(ChestTransferIntent packet)
  {
    if (packet.InventorySlot >= 10 || packet.ChestSlot >= 40)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write(packet.ChestId);
      writer.Write(packet.InventorySlot);
      writer.Write(packet.ChestSlot);
      writer.Write(packet.Withdraw);
      if (packet.ExpectedRevision < -1)
      {
        throw new ArgumentOutOfRangeException(nameof(packet));
      }

      if (packet.ExpectedRevision >= 0)
      {
        writer.Write(ChestTransferRevisionExtensionMarker);
        writer.Write(packet.ExpectedRevision);
      }
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncPlayerChest,
      payload.ToArray()));
  }

  private static long ReadExplicitChestTransferRevision(BinaryReader reader)
  {
    if (reader.ReadByte() != ChestTransferRevisionExtensionMarker)
    {
      throw new InvalidDataException(
        "Terraria SyncPlayerChest has an unknown revision extension marker.");
    }

    return reader.ReadInt64();
  }

  public static DoorToggleIntent DecodeDoorToggle(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.ToggleDoorState ||
        frame.Payload.Length != RequiredDoorToggleLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid ToggleDoorState packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new DoorToggleIntent(
      (DoorToggleAction)reader.ReadByte(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadByte() != 0);
  }

  public static byte[] EncodeDoorToggle(DoorToggleIntent packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((byte)packet.Action);
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
      writer.Write(packet.Direction);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ToggleDoorState,
      payload.ToArray()));
  }

  public static byte[] EncodeDoorState(DoorStateReplicationSnapshot snapshot)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((byte)snapshot.Action);
      writer.Write(snapshot.TileX);
      writer.Write(snapshot.TileY);
      writer.Write(snapshot.Direction);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ToggleDoorState,
      payload.ToArray()));
  }

  public static SignOpenRequestPacket DecodeSignOpenRequest(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.OpenSignRequest || frame.Payload.Length != 4)
    {
      throw new InvalidDataException("Terraria frame is not an OpenSignRequest packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new SignOpenRequestPacket(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      BinaryPrimitives.ReadInt16LittleEndian(payload[sizeof(short)..]));
  }

  public static byte[] EncodeSignOpenRequest(SignOpenRequestPacket packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.OpenSignRequest,
      payload.ToArray()));
  }

  public static SignUpdateIntent DecodeSignUpdate(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.OpenSignResponse)
    {
      throw new InvalidDataException("Terraria frame is not an OpenSignResponse packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    short signId = reader.ReadInt16();
    short tileX = reader.ReadInt16();
    short tileY = reader.ReadInt16();
    string text = reader.ReadString();
    byte playerSlot = reader.ReadByte();
    bool suppressOpenSign = (reader.ReadByte() & 1) != 0;
    SignUpdateIntent intent = new(
      playerSlot,
      signId,
      tileX,
      tileY,
      text,
      suppressOpenSign);
    if (intent.Text.Length > 100 || stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria OpenSignResponse has invalid text or trailing data.");
    }

    return intent;
  }

  public static byte[] EncodeSignUpdate(SignUpdateIntent packet)
  {
    ArgumentNullException.ThrowIfNull(packet.Text);
    if (packet.SignId < short.MinValue || packet.SignId > short.MaxValue || packet.Text.Length > 100)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)packet.SignId);
      writer.Write(packet.TileX);
      writer.Write(packet.TileY);
      writer.Write(packet.Text);
      writer.Write(packet.PlayerSlot);
      writer.Write(packet.SuppressOpenSign ? (byte)1 : (byte)0);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.OpenSignResponse,
      payload.ToArray()));
  }

  public static byte[] EncodeSignState(SignReplicationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot.Text);
    if (snapshot.SignId < short.MinValue || snapshot.SignId > short.MaxValue ||
        snapshot.Text.Length > 100)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.SignId);
      writer.Write(snapshot.TileX);
      writer.Write(snapshot.TileY);
      writer.Write(snapshot.Text);
      writer.Write(snapshot.PlayerSlot);
      writer.Write(snapshot.SuppressOpenSign ? (byte)1 : (byte)0);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.OpenSignResponse,
      payload.ToArray()));
  }

  public static RequestWorldDataPacket DecodeRequestWorldData(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RequestWorldData || frame.Payload.Length != 0)
    {
      throw new InvalidDataException("Terraria frame is not an empty RequestWorldData packet.");
    }

    return new RequestWorldDataPacket();
  }

  public static SpawnTileDataRequestPacket DecodeSpawnTileData(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SpawnTileData || frame.Payload.Length != 9)
    {
      throw new InvalidDataException("Terraria frame is not a valid SpawnTileData packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new SpawnTileDataRequestPacket(reader.ReadInt32(), reader.ReadInt32(), reader.ReadByte());
  }

  public static RequestSectionPacket DecodeRequestSection(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RequestSection || frame.Payload.Length != 4)
    {
      throw new InvalidDataException("Terraria frame is not a valid RequestSection packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new RequestSectionPacket(reader.ReadUInt16(), reader.ReadUInt16());
  }

  public static PlayerSpawnPacket DecodePlayerSpawn(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerSpawn || frame.Payload.Length != 15)
    {
      throw new InvalidDataException("Terraria frame is not a valid PlayerSpawn packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new PlayerSpawnPacket(
      reader.ReadByte(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadInt32(),
      reader.ReadInt16(),
      reader.ReadInt16(),
      reader.ReadByte(),
      reader.ReadByte());
  }

  public static WorldDataPacket DecodeWorldData(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.WorldData)
    {
      throw new InvalidDataException("Terraria frame is not a WorldData packet.");
    }

    return new WorldDataPacket(frame.Payload);
  }

  public static PlayerProfilePacket DecodePlayerProfile(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncPlayer)
    {
      throw new InvalidDataException("Terraria frame is not a SyncPlayer packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    PlayerProfilePacket packet = new(
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadSingle(),
      reader.ReadByte(),
      reader.ReadString(),
      reader.ReadByte(),
      reader.ReadUInt16(),
      reader.ReadByte(),
      ReadColor(reader),
      ReadColor(reader),
      ReadColor(reader),
      ReadColor(reader),
      ReadColor(reader),
      ReadColor(reader),
      ReadColor(reader),
      reader.ReadByte(),
      reader.ReadByte(),
      reader.ReadByte());
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria SyncPlayer packet has trailing data.");
    }

    return packet;
  }

  public static SetUserSlotPacket DecodeSetUserSlot(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SetUserSlot || frame.Payload.Length != 2)
    {
      throw new InvalidDataException("Terraria frame is not a valid SetUserSlot packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new SetUserSlotPacket(payload[0], payload[1] != 0);
  }

  public static PlayerEquipmentPacket DecodePlayerEquipment(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncEquipment ||
        frame.Payload.Length != RequiredEquipmentLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid SyncEquipment packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    byte playerSlot = reader.ReadByte();
    int slotId = reader.ReadInt16();
    int stack = reader.ReadInt16();
    byte prefix = reader.ReadByte();
    int itemType = reader.ReadInt16();
    byte flags = reader.ReadByte();
    if (slotId < 0 || slotId >= PlayerItemSlotCount || stack < 0)
    {
      throw new InvalidDataException("Terraria SyncEquipment references invalid item data.");
    }

    return new PlayerEquipmentPacket(
      playerSlot,
      slotId,
      stack,
      prefix,
      itemType,
      (flags & 1) != 0,
      (flags & 2) != 0);
  }

  public static PlayerBuffsPacket DecodePlayerBuffs(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerBuffs || frame.Payload.Length < 3 ||
        frame.Payload.Length % 2 == 0)
    {
      throw new InvalidDataException("Terraria frame is not a valid PlayerBuffs packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    byte playerSlot = payload[0];
    List<ushort> buffTypes = new();
    for (int offset = 1; offset < payload.Length; offset += sizeof(ushort))
    {
      ushort buffType = BitConverter.ToUInt16(payload[offset..]);
      if (buffType == 0)
      {
        if (offset + sizeof(ushort) != payload.Length)
        {
          throw new InvalidDataException("Terraria PlayerBuffs packet has trailing data.");
        }

        return new PlayerBuffsPacket(playerSlot, buffTypes);
      }

      if (buffTypes.Count == MaximumBuffCount)
      {
        throw new InvalidDataException("Terraria PlayerBuffs packet exceeds its buff limit.");
      }

      buffTypes.Add(buffType);
    }

    throw new InvalidDataException("Terraria PlayerBuffs packet is missing its terminator.");
  }

  public static AddPlayerBuffPvpPacket DecodeAddPlayerBuffPvp(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.AddPlayerBuffPvp ||
        frame.Payload.Length != RequiredAddPlayerBuffPvpLength)
    {
      throw new InvalidDataException(
        "Terraria frame is not a valid AddPlayerBuffPvP packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    byte playerSlot = payload[0];
    ushort buffType = BinaryPrimitives.ReadUInt16LittleEndian(payload[sizeof(byte)..]);
    int durationTicks = BinaryPrimitives.ReadInt32LittleEndian(
      payload[(sizeof(byte) + sizeof(ushort))..]);
    if (buffType == 0 || durationTicks <= 0)
    {
      throw new InvalidDataException(
        "Terraria AddPlayerBuffPvP packet has an invalid buff or duration.");
    }

    return new AddPlayerBuffPvpPacket(playerSlot, buffType, durationTicks);
  }

  public static PlayerLoadoutPacket DecodePlayerLoadout(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncLoadout ||
        frame.Payload.Length != RequiredLoadoutLength)
    {
      throw new InvalidDataException("Terraria frame is not a valid SyncLoadout packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new PlayerLoadoutPacket(payload[0], payload[1], BitConverter.ToUInt16(payload[2..]));
  }

  public static PlayerUuidPacket DecodePlayerUuid(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerUuid)
    {
      throw new InvalidDataException("Terraria frame is not a PlayerUuid packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    string value = reader.ReadString();
    if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumUuidLength ||
        !Guid.TryParseExact(value, "D", out _) || stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria PlayerUuid packet is invalid.");
    }

    return new PlayerUuidPacket(value);
  }

  public static TeleportEntityPacket DecodeTeleportEntity(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TeleportEntity || frame.Payload.Length < 12)
    {
      throw new InvalidDataException("Terraria frame is not a valid TeleportEntity packet.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    byte flags = payload[0];
    short targetId = BinaryPrimitives.ReadInt16LittleEndian(payload[1..]);
    float positionX = BitConverter.Int32BitsToSingle(
      BinaryPrimitives.ReadInt32LittleEndian(payload[3..]));
    float positionY = BitConverter.Int32BitsToSingle(
      BinaryPrimitives.ReadInt32LittleEndian(payload[7..]));
    byte style = payload[11];
    int? extraInfo = null;
    if ((flags & 0x08) != 0)
    {
      if (payload.Length != 16)
      {
        throw new InvalidDataException(
          "Terraria TeleportEntity packet has an invalid optional extra-info boundary.");
      }

      extraInfo = BinaryPrimitives.ReadInt32LittleEndian(payload[12..]);
    }
    else if (payload.Length != 12)
    {
      throw new InvalidDataException(
        "Terraria TeleportEntity packet contains unexpected trailing bytes.");
    }

    return new TeleportEntityPacket(flags, targetId, positionX, positionY, style, extraInfo);
  }

  public static PlayerHealOtherPacket DecodePlayerHealOther(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerHealOther || frame.Payload.Length != 3)
    {
      throw new InvalidDataException(
        "Terraria PlayerHealOther packet must contain a three-byte payload.");
    }

    return new PlayerHealOtherPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[1..]));
  }

  public static PlayerStealthPacket DecodePlayerStealth(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PlayerStealth || frame.Payload.Length != 5)
    {
      throw new InvalidDataException(
        "Terraria PlayerStealth packet must contain a five-byte payload.");
    }

    return new PlayerStealthPacket(
      frame.Payload.Span[0],
      BitConverter.Int32BitsToSingle(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span[1..])));
  }

  public static SyncExtraValuePacket DecodeSyncExtraValue(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncExtraValue || frame.Payload.Length != 14)
    {
      throw new InvalidDataException(
        "Terraria SyncExtraValue packet must contain a fourteen-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new SyncExtraValuePacket(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      BinaryPrimitives.ReadInt32LittleEndian(payload[2..]),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[6..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[10..])));
  }

  public static TeleportPlayerThroughPortalPacket DecodeTeleportPlayerThroughPortal(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TeleportPlayerThroughPortal ||
        frame.Payload.Length != 19)
    {
      throw new InvalidDataException(
        "Terraria TeleportPlayerThroughPortal packet must contain a nineteen-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new TeleportPlayerThroughPortalPacket(
      payload[0],
      BinaryPrimitives.ReadInt16LittleEndian(payload[1..]),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[3..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[7..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[11..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[15..])));
  }

  public static MinionRestTargetUpdatePacket DecodeMinionRestTargetUpdate(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.MinionRestTargetUpdate ||
        frame.Payload.Length != 9)
    {
      throw new InvalidDataException(
        "Terraria MinionRestTargetUpdate packet must contain a nine-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new MinionRestTargetUpdatePacket(
      payload[0],
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[1..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[5..])));
  }

  public static TeleportNpcThroughPortalPacket DecodeTeleportNpcThroughPortal(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TeleportNpcThroughPortal ||
        frame.Payload.Length != 20)
    {
      throw new InvalidDataException(
        "Terraria TeleportNpcThroughPortal packet must contain a twenty-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new TeleportNpcThroughPortalPacket(
      BinaryPrimitives.ReadUInt16LittleEndian(payload),
      BinaryPrimitives.ReadInt16LittleEndian(payload[2..]),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[4..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[8..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[12..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[16..])));
  }

  public static NebulaLevelupRequestPacket DecodeNebulaLevelupRequest(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.NebulaLevelupRequest ||
        frame.Payload.Length != 11)
    {
      throw new InvalidDataException(
        "Terraria NebulaLevelupRequest packet must contain an eleven-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new NebulaLevelupRequestPacket(
      payload[0],
      BinaryPrimitives.ReadUInt16LittleEndian(payload[1..]),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[3..])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[7..])));
  }

  public static MoonlordHorrorPacket DecodeMoonlordHorror(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.MoonlordHorror || frame.Payload.Length != 8)
    {
      throw new InvalidDataException(
        "Terraria MoonlordHorror packet must contain an eight-byte payload.");
    }

    return new MoonlordHorrorPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span),
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span[4..]));
  }

  public static ShopOverridePacket DecodeShopOverride(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.ShopOverride || frame.Payload.Length != 11)
    {
      throw new InvalidDataException(
        "Terraria ShopOverride packet must contain an eleven-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new ShopOverridePacket(
      payload[0],
      BinaryPrimitives.ReadInt16LittleEndian(payload[1..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[3..]),
      payload[5],
      BinaryPrimitives.ReadInt32LittleEndian(payload[6..]),
      payload[10]);
  }

  public static GemLockTogglePacket DecodeGemLockToggle(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.GemLockToggle || frame.Payload.Length != 5)
    {
      throw new InvalidDataException(
        "Terraria GemLockToggle packet must contain a five-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new GemLockTogglePacket(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      BinaryPrimitives.ReadInt16LittleEndian(payload[2..]),
      payload[4] != 0);
  }

  public static PoofOfSmokePacket DecodePoofOfSmoke(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.PoofOfSmoke || frame.Payload.Length != 4)
    {
      throw new InvalidDataException(
        "Terraria PoofOfSmoke packet must contain a four-byte packed payload.");
    }

    return new PoofOfSmokePacket(BinaryPrimitives.ReadUInt32LittleEndian(frame.Payload.Span));
  }

  public static WiredCannonShotPacket DecodeWiredCannonShot(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.WiredCannonShot || frame.Payload.Length != 15)
    {
      throw new InvalidDataException(
        "Terraria WiredCannonShot packet must contain a fifteen-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new WiredCannonShotPacket(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(payload[2..])),
      BinaryPrimitives.ReadInt16LittleEndian(payload[6..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[8..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[10..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[12..]),
      payload[14]);
  }

  public static MassWireOperationPacket DecodeMassWireOperation(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.MassWireOperation || frame.Payload.Length != 9)
    {
      throw new InvalidDataException(
        "Terraria MassWireOperation packet must contain a nine-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new MassWireOperationPacket(
      BinaryPrimitives.ReadInt16LittleEndian(payload),
      BinaryPrimitives.ReadInt16LittleEndian(payload[2..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[4..]),
      BinaryPrimitives.ReadInt16LittleEndian(payload[6..]),
      payload[8]);
  }

  public static MassWireOperationPayPacket DecodeMassWireOperationPay(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.MassWireOperationPay || frame.Payload.Length != 5)
    {
      throw new InvalidDataException(
        "Terraria MassWireOperationPay packet must contain a five-byte payload.");
    }

    return new MassWireOperationPayPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[2..]),
      frame.Payload.Span[4]);
  }

  public static SpecialFxPacket DecodeSpecialFx(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SpecialFx || frame.Payload.Length != 13)
    {
      throw new InvalidDataException(
        "Terraria SpecialFX packet must contain a thirteen-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new SpecialFxPacket(
      payload[0],
      BinaryPrimitives.ReadInt32LittleEndian(payload[1..]),
      BinaryPrimitives.ReadInt32LittleEndian(payload[5..]),
      payload[9],
      BinaryPrimitives.ReadInt16LittleEndian(payload[10..]),
      payload[12]);
  }

  public static CrystalInvasionStartPacket DecodeCrystalInvasionStart(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.CrystalInvasionStart ||
        frame.Payload.Length != 4)
    {
      throw new InvalidDataException(
        "Terraria CrystalInvasionStart packet must contain a four-byte payload.");
    }

    return new CrystalInvasionStartPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[2..]));
  }

  public static CrystalInvasionWipePacket DecodeCrystalInvasionWipe(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.CrystalInvasionWipeAllTheThingsss ||
        frame.Payload.Length != 0)
    {
      throw new InvalidDataException(
        "Terraria CrystalInvasionWipe packet must contain an empty payload.");
    }

    return new CrystalInvasionWipePacket();
  }

  public static MinionAttackTargetUpdatePacket DecodeMinionAttackTargetUpdate(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.MinionAttackTargetUpdate ||
        frame.Payload.Length != 3)
    {
      throw new InvalidDataException(
        "Terraria MinionAttackTargetUpdate packet must contain a three-byte payload.");
    }

    return new MinionAttackTargetUpdatePacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[1..]));
  }

  public static CrystalInvasionNextWaveWaitPacket DecodeCrystalInvasionNextWaveWait(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.CrystalInvasionSendWaitTime ||
        frame.Payload.Length != sizeof(int))
    {
      throw new InvalidDataException(
        "Terraria CrystalInvasionSendWaitTime packet must contain a four-byte payload.");
    }

    return new CrystalInvasionNextWaveWaitPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span));
  }

  public static PlayerHurtV2Packet DecodePlayerHurtV2(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int trailerLength = sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(sbyte);
    if (frame.MessageId != TerrariaMessageId.PlayerHurtV2 ||
        frame.Payload.Length < sizeof(byte) + sizeof(byte) + trailerLength)
    {
      throw new InvalidDataException(
        "Terraria PlayerHurtV2 packet must contain a variable death-reason payload and five trailing bytes.");
    }

    int reasonLength = frame.Payload.Length - sizeof(byte) - trailerLength;
    byte[] reason = frame.Payload.Span.Slice(sizeof(byte), reasonLength).ToArray();
    int tailOffset = sizeof(byte) + reasonLength;
    return new PlayerHurtV2Packet(
      frame.Payload.Span[0],
      reason,
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(tailOffset, sizeof(short))),
      frame.Payload.Span[tailOffset + sizeof(short)],
      frame.Payload.Span[tailOffset + sizeof(short) + sizeof(byte)],
      unchecked((sbyte)frame.Payload.Span[tailOffset + sizeof(short) + sizeof(byte) + sizeof(byte)]));
  }

  public static PlayerDeathV2Packet DecodePlayerDeathV2(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int trailerLength = sizeof(short) + sizeof(byte) + sizeof(byte);
    if (frame.MessageId != TerrariaMessageId.PlayerDeathV2 ||
        frame.Payload.Length < sizeof(byte) + sizeof(byte) + trailerLength)
    {
      throw new InvalidDataException(
        "Terraria PlayerDeathV2 packet must contain a variable death-reason payload and three trailing bytes.");
    }

    int reasonLength = frame.Payload.Length - sizeof(byte) - trailerLength;
    byte[] reason = frame.Payload.Span.Slice(sizeof(byte), reasonLength).ToArray();
    int tailOffset = sizeof(byte) + reasonLength;
    return new PlayerDeathV2Packet(
      frame.Payload.Span[0],
      reason,
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(tailOffset, sizeof(short))),
      frame.Payload.Span[tailOffset + sizeof(short)],
      frame.Payload.Span[tailOffset + sizeof(short) + sizeof(byte)]);
  }

  public static CombatTextStringPacket DecodeCombatTextString(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int prefixLength = sizeof(float) + sizeof(float) + 3;
    if (frame.MessageId != TerrariaMessageId.CombatTextString ||
        frame.Payload.Length <= prefixLength)
    {
      throw new InvalidDataException(
        "Terraria CombatTextString packet must contain an RGB prefix and variable text payload.");
    }

    return new CombatTextStringPacket(
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span[..sizeof(float)])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span.Slice(sizeof(float), sizeof(float)))),
      new TerrariaColor(
        frame.Payload.Span[8], frame.Payload.Span[9], frame.Payload.Span[10]),
      frame.Payload.Span[prefixLength..].ToArray());
  }

  public static EmojiPacket DecodeEmoji(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.Emoji || frame.Payload.Length != 2)
    {
      throw new InvalidDataException("Terraria Emoji packet must contain a two-byte payload.");
    }

    return new EmojiPacket(frame.Payload.Span[0], frame.Payload.Span[1]);
  }

  public static DisplayDollDataSyncPacket DecodeDisplayDollDataSync(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int prefixLength = sizeof(byte) + sizeof(int) + sizeof(byte) + sizeof(byte);
    if (frame.MessageId != TerrariaMessageId.TedisplayDollDataSync ||
        frame.Payload.Length <= prefixLength)
    {
      throw new InvalidDataException(
        "Terraria TEDisplayDollDataSync packet must contain a variable data payload.");
    }

    return new DisplayDollDataSyncPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(sizeof(byte), sizeof(int))),
      frame.Payload.Span[sizeof(byte) + sizeof(int)],
      frame.Payload.Span[sizeof(byte) + sizeof(int) + sizeof(byte)],
      frame.Payload.Span[prefixLength..].ToArray());
  }

  public static RequestTileEntityInteractionPacket DecodeRequestTileEntityInteraction(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RequestTileEntityInteraction ||
        frame.Payload.Length != sizeof(int) + sizeof(byte))
    {
      throw new InvalidDataException(
        "Terraria RequestTileEntityInteraction packet must contain a five-byte payload.");
    }

    return new RequestTileEntityInteractionPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span[..sizeof(int)]),
      frame.Payload.Span[sizeof(int)]);
  }

  public static WeaponsRackTryPlacingPacket DecodeWeaponsRackTryPlacing(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.WeaponsRackTryPlacing || frame.Payload.Length != 9)
    {
      throw new InvalidDataException(
        "Terraria WeaponsRackTryPlacing packet must contain a nine-byte payload.");
    }

    return new WeaponsRackTryPlacingPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[..sizeof(short)]),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(short), sizeof(short))),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(short) * 2, sizeof(short))),
      frame.Payload.Span[sizeof(short) * 3],
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(short) * 3 + sizeof(byte), sizeof(short))));
  }

  public static HatRackItemSyncPacket DecodeHatRackItemSync(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.TehatRackItemSync || frame.Payload.Length != 11)
    {
      throw new InvalidDataException(
        "Terraria TEHatRackItemSync packet must contain an eleven-byte payload.");
    }

    byte encodedSlot = frame.Payload.Span[sizeof(byte) + sizeof(int)];
    bool isDye = encodedSlot >= 2;
    byte slot = isDye ? (byte)(encodedSlot - 2) : encodedSlot;
    return new HatRackItemSyncPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(sizeof(byte), sizeof(int))),
      slot,
      isDye,
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(sizeof(byte) + sizeof(int) + sizeof(byte), sizeof(int))),
      frame.Payload.Span[sizeof(byte) + sizeof(int) + sizeof(byte) + sizeof(int)]);
  }

  public static SyncPlayerChestLocationPacket DecodeSyncPlayerChestLocation(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncPlayerChestLocation || frame.Payload.Length != 6)
    {
      throw new InvalidDataException(
        "Terraria SyncPlayerChestLocation packet must contain a six-byte payload.");
    }

    return new SyncPlayerChestLocationPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte), sizeof(short))),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte) + sizeof(short), sizeof(short))),
      frame.Payload.Span[sizeof(byte) + sizeof(short) * 2]);
  }

  public static SyncRevengeMarkerPacket DecodeSyncRevengeMarker(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncRevengeMarker ||
        frame.Payload.Length != sizeof(short))
    {
      throw new InvalidDataException(
        "Terraria SyncRevengeMarker packet must contain a two-byte payload.");
    }

    return new SyncRevengeMarkerPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span));
  }

  public static RemoveRevengeMarkerPacket DecodeRemoveRevengeMarker(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RemoveRevengeMarker ||
        frame.Payload.Length != sizeof(int))
    {
      throw new InvalidDataException(
        "Terraria RemoveRevengeMarker packet must contain a four-byte payload.");
    }

    return new RemoveRevengeMarkerPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span));
  }

  public static LandGolfBallInCupPacket DecodeLandGolfBallInCup(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.LandGolfBallInCup ||
        frame.Payload.Length != sizeof(byte) + sizeof(ushort) * 4)
    {
      throw new InvalidDataException(
        "Terraria LandGolfBallInCup packet must contain a nine-byte payload.");
    }

    return new LandGolfBallInCupPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte), sizeof(ushort))),
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte) + sizeof(ushort), sizeof(ushort))),
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte) + sizeof(ushort) * 2, sizeof(ushort))),
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte) + sizeof(ushort) * 3, sizeof(ushort))));
  }

  public static FishOutNpcPacket DecodeFishOutNpc(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.FishOutNpc ||
        frame.Payload.Length != sizeof(ushort) * 2 + sizeof(short))
    {
      throw new InvalidDataException("Terraria FishOutNPC packet must contain a six-byte payload.");
    }

    return new FishOutNpcPacket(
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span[..sizeof(ushort)]),
      BinaryPrimitives.ReadUInt16LittleEndian(frame.Payload.Span.Slice(sizeof(ushort), sizeof(ushort))),
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(ushort) * 2, sizeof(short))));
  }

  public static PlayerVitalsPacket DecodePlayerLifeMana(ReadOnlySpan<byte> frameBytes)
  {
    return DecodePlayerVitals(frameBytes, TerrariaMessageId.PlayerLifeMana, "PlayerLifeMana");
  }

  public static PlayerVitalsPacket DecodePlayerMana(ReadOnlySpan<byte> frameBytes)
  {
    return DecodePlayerVitals(
      frameBytes,
      TerrariaMessageId.ItemRotationAndAnimation,
      "player mana");
  }

  public static HelloPacket DecodeHello(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.Hello)
    {
      throw new InvalidDataException("Terraria frame is not a Hello packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    string protocolIdentifier = reader.ReadString();
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria Hello packet has trailing data.");
    }

    return new HelloPacket(protocolIdentifier);
  }

  public static byte[] Encode(HelloPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet.ProtocolIdentifier);

    using MemoryStream payloadStream = new();
    using (BinaryWriter writer = new(payloadStream, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.ProtocolIdentifier);
    }

    TerrariaFrame frame = new(TerrariaMessageId.Hello, payloadStream.ToArray());
    return TerrariaFrameCodec.Encode(frame);
  }

  public static byte[] Encode(PlayerProfilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet.Name);

    using MemoryStream payloadStream = new();
    using (BinaryWriter writer = new(payloadStream, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write(packet.SkinVariant);
      writer.Write(packet.VoiceVariant);
      writer.Write(packet.VoicePitchOffset);
      writer.Write(packet.Hair);
      writer.Write(packet.Name);
      writer.Write(packet.HairDye);
      writer.Write(packet.AccessoryVisibility);
      writer.Write(packet.HideMisc);
      WriteColor(writer, packet.HairColor);
      WriteColor(writer, packet.SkinColor);
      WriteColor(writer, packet.EyeColor);
      WriteColor(writer, packet.ShirtColor);
      WriteColor(writer, packet.UnderShirtColor);
      WriteColor(writer, packet.PantsColor);
      WriteColor(writer, packet.ShoeColor);
      writer.Write(packet.DifficultyFlags);
      writer.Write(packet.BiomeTorchFlags);
      writer.Write(packet.ConsumableFlags);
    }

    TerrariaFrame frame = new(TerrariaMessageId.SyncPlayer, payloadStream.ToArray());
    return TerrariaFrameCodec.Encode(frame);
  }

  public static byte[] Encode(PlayerUuidPacket packet)
  {
    if (string.IsNullOrWhiteSpace(packet.Value) || packet.Value.Length > MaximumUuidLength ||
        !Guid.TryParseExact(packet.Value, "D", out _))
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.Value);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.PlayerUuid, payload.ToArray()));
  }

  public static byte[] Encode(TeleportEntityPacket packet)
  {
    bool hasExtraInfo = (packet.Flags & 0x08) != 0;
    if (hasExtraInfo != packet.ExtraInfo.HasValue)
    {
      throw new ArgumentException(
        "TeleportEntity extra info must match flag bit 3.",
        nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.Flags);
      writer.Write(packet.TargetId);
      writer.Write(packet.PositionX);
      writer.Write(packet.PositionY);
      writer.Write(packet.Style);
      if (packet.ExtraInfo is int extraInfo)
      {
        writer.Write(extraInfo);
      }
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TeleportEntity,
      payload.ToArray()));
  }

  public static byte[] Encode(PlayerHealOtherPacket packet)
  {
    byte[] payload = new byte[3];
    payload[0] = packet.PlayerId;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(1), packet.Amount);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerHealOther,
      payload));
  }

  public static byte[] Encode(PlayerStealthPacket packet)
  {
    byte[] payload = new byte[5];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(1),
      BitConverter.SingleToInt32Bits(packet.Stealth));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerStealth,
      payload));
  }

  public static byte[] Encode(SyncExtraValuePacket packet)
  {
    byte[] payload = new byte[14];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.NpcId);
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(2), packet.ExtraValue);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(6),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(10),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncExtraValue,
      payload));
  }

  public static byte[] Encode(TeleportPlayerThroughPortalPacket packet)
  {
    byte[] payload = new byte[19];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(1), packet.PortalColor);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(3),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(7),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(11),
      BitConverter.SingleToInt32Bits(packet.VelocityX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(15),
      BitConverter.SingleToInt32Bits(packet.VelocityY));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TeleportPlayerThroughPortal,
      payload));
  }

  public static byte[] Encode(MinionRestTargetUpdatePacket packet)
  {
    byte[] payload = new byte[9];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(1),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(5),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.MinionRestTargetUpdate,
      payload));
  }

  public static byte[] Encode(TeleportNpcThroughPortalPacket packet)
  {
    byte[] payload = new byte[20];
    BinaryPrimitives.WriteUInt16LittleEndian(payload, packet.NpcId);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), packet.PortalColor);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(4),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(8),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(12),
      BitConverter.SingleToInt32Bits(packet.VelocityX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(16),
      BitConverter.SingleToInt32Bits(packet.VelocityY));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TeleportNpcThroughPortal,
      payload));
  }

  public static byte[] Encode(NebulaLevelupRequestPacket packet)
  {
    byte[] payload = new byte[11];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(1), packet.PowerId);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(3),
      BitConverter.SingleToInt32Bits(packet.TargetX));
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(7),
      BitConverter.SingleToInt32Bits(packet.TargetY));
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.NebulaLevelupRequest,
      payload));
  }

  public static byte[] Encode(MoonlordHorrorPacket packet)
  {
    byte[] payload = new byte[8];
    BinaryPrimitives.WriteInt32LittleEndian(payload, packet.MaximumCountdown);
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(4), packet.Countdown);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.MoonlordHorror,
      payload));
  }

  public static byte[] Encode(ShopOverridePacket packet)
  {
    byte[] payload = new byte[11];
    payload[0] = packet.Slot;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(1), packet.Type);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(3), packet.Stack);
    payload[5] = packet.Prefix;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(6), packet.Value);
    payload[10] = packet.Flags;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ShopOverride,
      payload));
  }

  public static byte[] Encode(GemLockTogglePacket packet)
  {
    byte[] payload = new byte[5];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.TileX);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), packet.TileY);
    payload[4] = packet.Locked ? (byte)1 : (byte)0;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.GemLockToggle,
      payload));
  }

  public static byte[] Encode(PoofOfSmokePacket packet)
  {
    byte[] payload = new byte[4];
    BinaryPrimitives.WriteUInt32LittleEndian(payload, packet.PackedValue);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PoofOfSmoke,
      payload));
  }

  public static byte[] Encode(WiredCannonShotPacket packet)
  {
    byte[] payload = new byte[15];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.Damage);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(2),
      BitConverter.SingleToInt32Bits(packet.Knockback));
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(6), packet.X);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(8), packet.Y);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(10), packet.Angle);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(12), packet.Ammo);
    payload[14] = packet.PlayerId;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.WiredCannonShot,
      payload));
  }

  public static byte[] Encode(MassWireOperationPacket packet)
  {
    byte[] payload = new byte[9];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.StartX);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), packet.StartY);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(4), packet.EndX);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(6), packet.EndY);
    payload[8] = packet.ToolMode;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.MassWireOperation,
      payload));
  }

  public static byte[] Encode(MassWireOperationPayPacket packet)
  {
    byte[] payload = new byte[5];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.ItemType);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), packet.Count);
    payload[4] = packet.PlayerId;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.MassWireOperationPay,
      payload));
  }

  public static byte[] Encode(SpecialFxPacket packet)
  {
    byte[] payload = new byte[13];
    payload[0] = packet.EffectType;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1), packet.Number2);
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(5), packet.Number3);
    payload[9] = packet.Number4;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(10), packet.Number5);
    payload[12] = packet.Number6;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SpecialFx,
      payload));
  }

  public static byte[] Encode(CrystalInvasionStartPacket packet)
  {
    byte[] payload = new byte[4];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.X);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(2), packet.Y);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.CrystalInvasionStart,
      payload));
  }

  public static byte[] Encode(CrystalInvasionWipePacket packet)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.CrystalInvasionWipeAllTheThingsss,
      Array.Empty<byte>()));
  }

  public static byte[] Encode(MinionAttackTargetUpdatePacket packet)
  {
    byte[] payload = new byte[3];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(1), packet.TargetId);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.MinionAttackTargetUpdate,
      payload));
  }

  public static byte[] Encode(CrystalInvasionNextWaveWaitPacket packet)
  {
    byte[] payload = new byte[sizeof(int)];
    BinaryPrimitives.WriteInt32LittleEndian(payload, packet.TimeLeft);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.CrystalInvasionSendWaitTime,
      payload));
  }

  public static byte[] Encode(PlayerHurtV2Packet packet)
  {
    ArgumentNullException.ThrowIfNull(packet.DeathReasonPayload);
    byte[] payload = new byte[sizeof(byte) + packet.DeathReasonPayload.Length +
      sizeof(short) + sizeof(byte) + sizeof(byte) + sizeof(sbyte)];
    payload[0] = packet.PlayerId;
    packet.DeathReasonPayload.CopyTo(payload, sizeof(byte));
    int tailOffset = sizeof(byte) + packet.DeathReasonPayload.Length;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(tailOffset, sizeof(short)), packet.Damage);
    payload[tailOffset + sizeof(short)] = packet.Direction;
    payload[tailOffset + sizeof(short) + sizeof(byte)] = packet.Flags;
    payload[tailOffset + sizeof(short) + sizeof(byte) + sizeof(byte)] =
      unchecked((byte)packet.CooldownCounter);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.PlayerHurtV2, payload));
  }

  public static byte[] Encode(PlayerDeathV2Packet packet)
  {
    ArgumentNullException.ThrowIfNull(packet.DeathReasonPayload);
    byte[] payload = new byte[sizeof(byte) + packet.DeathReasonPayload.Length +
      sizeof(short) + sizeof(byte) + sizeof(byte)];
    payload[0] = packet.PlayerId;
    packet.DeathReasonPayload.CopyTo(payload, sizeof(byte));
    int tailOffset = sizeof(byte) + packet.DeathReasonPayload.Length;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(tailOffset, sizeof(short)), packet.Damage);
    payload[tailOffset + sizeof(short)] = packet.Direction;
    payload[tailOffset + sizeof(short) + sizeof(byte)] = packet.Pvp;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.PlayerDeathV2, payload));
  }

  public static byte[] Encode(CombatTextStringPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet.TextPayload);
    if (packet.TextPayload.Length == 0)
    {
      throw new ArgumentException("CombatTextString requires a text payload.", nameof(packet));
    }

    const int prefixLength = sizeof(float) + sizeof(float) + 3;
    byte[] payload = new byte[prefixLength + packet.TextPayload.Length];
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0, sizeof(float)),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(float), sizeof(float)),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    payload[8] = packet.Color.Red;
    payload[9] = packet.Color.Green;
    payload[10] = packet.Color.Blue;
    packet.TextPayload.CopyTo(payload, prefixLength);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.CombatTextString, payload));
  }

  public static byte[] Encode(EmojiPacket packet)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.Emoji,
      new[] { packet.PlayerId, packet.EmoteId }));
  }

  public static byte[] Encode(DisplayDollDataSyncPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet.DataPayload);
    if (packet.DataPayload.Length == 0)
    {
      throw new ArgumentException("TEDisplayDollDataSync requires data payload bytes.", nameof(packet));
    }

    const int prefixLength = sizeof(byte) + sizeof(int) + sizeof(byte) + sizeof(byte);
    byte[] payload = new byte[prefixLength + packet.DataPayload.Length];
    payload[0] = packet.PlayerId;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(byte), sizeof(int)), packet.EntityId);
    payload[sizeof(byte) + sizeof(int)] = packet.Slot;
    payload[sizeof(byte) + sizeof(int) + sizeof(byte)] = packet.Param;
    packet.DataPayload.CopyTo(payload, prefixLength);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TedisplayDollDataSync,
      payload));
  }

  public static byte[] Encode(RequestTileEntityInteractionPacket packet)
  {
    byte[] payload = new byte[sizeof(int) + sizeof(byte)];
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0, sizeof(int)), packet.EntityId);
    payload[sizeof(int)] = packet.PlayerId;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.RequestTileEntityInteraction,
      payload));
  }

  public static byte[] Encode(WeaponsRackTryPlacingPacket packet)
  {
    byte[] payload = new byte[9];
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(0, sizeof(short)), packet.X);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(short), sizeof(short)), packet.Y);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(short) * 2, sizeof(short)), packet.ItemType);
    payload[sizeof(short) * 3] = packet.Prefix;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(short) * 3 + sizeof(byte), sizeof(short)), packet.Stack);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.WeaponsRackTryPlacing, payload));
  }

  public static byte[] Encode(HatRackItemSyncPacket packet)
  {
    if (packet.Slot > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    byte[] payload = new byte[11];
    payload[0] = packet.PlayerId;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(byte), sizeof(int)), packet.EntityId);
    payload[sizeof(byte) + sizeof(int)] = packet.IsDye ? (byte)(packet.Slot + 2) : packet.Slot;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(byte) + sizeof(int) + sizeof(byte), sizeof(int)), packet.ItemType);
    payload[sizeof(byte) + sizeof(int) + sizeof(byte) + sizeof(int)] = packet.Prefix;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.TehatRackItemSync, payload));
  }

  public static byte[] Encode(PlayerBuffsPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet.BuffTypes);
    if (packet.BuffTypes.Count > MaximumBuffCount)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      for (int index = 0; index < packet.BuffTypes.Count; index++)
      {
        writer.Write(packet.BuffTypes[index]);
      }

      writer.Write((ushort)0);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.PlayerBuffs, payload.ToArray()));
  }

  public static byte[] Encode(AddPlayerBuffPvpPacket packet)
  {
    if (packet.BuffType == 0 || packet.DurationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    byte[] payload = new byte[RequiredAddPlayerBuffPvpLength];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteUInt16LittleEndian(
      payload.AsSpan(sizeof(byte), sizeof(ushort)), packet.BuffType);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(sizeof(byte) + sizeof(ushort), sizeof(int)), packet.DurationTicks);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.AddPlayerBuffPvp,
      payload));
  }

  public static byte[] Encode(PlayerEquipmentPacket packet)
  {
    if (packet.SlotId < 0 || packet.SlotId >= PlayerItemSlotCount || packet.Stack < 0 ||
        packet.Stack > short.MaxValue || packet.ItemType < 0 || packet.ItemType > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    byte flags = 0;
    if (packet.IsFavorited)
    {
      flags |= 1;
    }

    if (packet.IsNewAndShiny)
    {
      flags |= 2;
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write((short)packet.SlotId);
      writer.Write((short)packet.Stack);
      writer.Write(packet.Prefix);
      writer.Write((short)packet.ItemType);
      writer.Write(flags);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.SyncEquipment, payload.ToArray()));
  }

  public static byte[] Encode(PlayerLoadoutPacket packet)
  {
    if (packet.SelectedLoadout > 2)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    byte[] payload = [
      packet.PlayerSlot,
      packet.SelectedLoadout,
      (byte)packet.AccessoryVisibility,
      (byte)(packet.AccessoryVisibility >> 8)
    ];
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.SyncLoadout, payload));
  }

  public static byte[] EncodePlayerControls(
    PlayerControlIntent packet,
    float positionX,
    float positionY,
    ushort? mountType = null)
  {
    byte controlFlags = 0;
    if (packet.Down)
    {
      controlFlags |= ControlDownBit;
    }

    if (packet.MoveLeft)
    {
      controlFlags |= ControlLeftBit;
    }

    if (packet.MoveRight)
    {
      controlFlags |= ControlRightBit;
    }

    if (packet.Jump)
    {
      controlFlags |= ControlJumpBit;
    }

    if (packet.UseItem)
    {
      controlFlags |= ControlUseItemBit;
    }

    if (packet.FacingRight)
    {
      controlFlags |= FacingRightBit;
    }

    byte secondaryFlags = 0;
    if (mountType is ushort)
    {
      secondaryFlags |= PlayerControlsMountBit;
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write(controlFlags);
      writer.Write(secondaryFlags);
      writer.Write((byte)0);
      writer.Write((byte)0);
      writer.Write(packet.SelectedItem);
      writer.Write(positionX);
      writer.Write(positionY);
      if (mountType is ushort value)
      {
        writer.Write(value);
      }
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerControls,
      payload.ToArray()));
  }

  public static byte[] EncodePlayerActive(byte playerSlot, bool isActive)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerActive,
      new byte[] { playerSlot, isActive ? (byte)1 : (byte)0 }));
  }

  public static byte[] EncodeWorldTime(WorldRuleSnapshot snapshot)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(snapshot.IsDayTime);
      writer.Write(ProjectWorldTimeToLegacy(snapshot.TimeOfDay));
      writer.Write(0.0f);
      writer.Write(0.0f);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.SetTime, payload.ToArray()));
  }

  private static int ProjectWorldTimeToLegacy(double timeOfDay)
  {
    if (!double.IsFinite(timeOfDay) || timeOfDay < int.MinValue || timeOfDay > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(timeOfDay));
    }

    return checked((int)Math.Truncate(timeOfDay));
  }

  public static byte[] EncodeAnglerQuest(WorldJoinStateSnapshot snapshot)
  {
    return EncodeAnglerQuest(new AnglerQuestPacket(snapshot.AnglerQuest, FinishedToday: false));
  }

  public static byte[] EncodeAnglerQuest(AnglerQuestPacket packet)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.AnglerQuest,
      new byte[] { packet.QuestId, packet.FinishedToday ? (byte)1 : (byte)0 }));
  }

  public static AnglerQuestPacket DecodeAnglerQuest(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.AnglerQuest || frame.Payload.Length != 2 ||
        frame.Payload.Span[1] > 1)
    {
      throw new InvalidDataException(
        "Terraria AnglerQuest packet must contain a quest byte and a boolean completion byte.");
    }

    return new AnglerQuestPacket(frame.Payload.Span[0], frame.Payload.Span[1] != 0);
  }

  public static byte[] EncodeCavernMonsterTypes(WorldJoinStateSnapshot snapshot)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(snapshot.CavernMonsterTypeOne);
      writer.Write(snapshot.CavernMonsterTypeTwo);
      writer.Write(snapshot.CavernMonsterTypeThree);
      writer.Write(snapshot.CavernMonsterTypeFour);
      writer.Write(snapshot.CavernMonsterTypeFive);
      writer.Write(snapshot.CavernMonsterTypeSix);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.CavernMonsterTypes,
      payload.ToArray()));
  }

  public static byte[] EncodeHostStatus(byte playerSlot, bool isHost)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.HostStatus,
      new byte[] { playerSlot, isHost ? (byte)1 : (byte)0 }));
  }

  public static byte[] EncodeTowerShieldStrengths(WorldJoinStateSnapshot snapshot)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(snapshot.SolarTowerShieldStrength);
      writer.Write(snapshot.VortexTowerShieldStrength);
      writer.Write(snapshot.NebulaTowerShieldStrength);
      writer.Write(snapshot.StardustTowerShieldStrength);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TowerShieldStrengths,
      payload.ToArray()));
  }

  public static byte[] EncodeWorldBiomeTypes(WorldJoinStateSnapshot snapshot)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.WorldBiomeTypes,
      new byte[] {
        snapshot.GoodBiomeTileType,
        snapshot.EvilBiomeTileType,
        snapshot.BloodBiomeTileType
      }));
  }

  public static byte[] EncodePlayerLifeMana(byte playerSlot, int health, int maximumHealth)
  {
    if (health < short.MinValue || health > short.MaxValue ||
        maximumHealth < short.MinValue || maximumHealth > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(health));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(playerSlot);
      writer.Write((short)health);
      writer.Write((short)maximumHealth);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerLifeMana,
      payload.ToArray()));
  }

  public static byte[] EncodePlayerMana(byte playerSlot, int mana, int maximumMana)
  {
    if (mana < short.MinValue || mana > short.MaxValue || maximumMana < short.MinValue ||
        maximumMana > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(mana));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(playerSlot);
      writer.Write((short)mana);
      writer.Write((short)maximumMana);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.ItemRotationAndAnimation,
      payload.ToArray()));
  }

  public static byte[] EncodeNpcReplication(NpcReplicationSnapshot snapshot)
  {
    return TerrariaV1456Compatibility.EncodeNpcReplication(
      LegacyNpcWireState.CreateDefault(snapshot));
  }

  public static byte[] EncodeNpcBuffs(NpcReplicationSnapshot snapshot)
  {
    return EncodeNpcBuffs(snapshot, Array.Empty<NpcBuffEntry>());
  }

  public static byte[] EncodeNpcBuffs(
    NpcReplicationSnapshot snapshot,
    IReadOnlyList<NpcBuffEntry> buffs)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    if (snapshot.ReplicationId > short.MaxValue || snapshot.ReplicationId < short.MinValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.ReplicationId);
      for (int index = 0; index < buffs.Count; index++)
      {
        NpcBuffEntry buff = buffs[index];
        if (buff.Type == 0 || buff.Duration == 0)
        {
          throw new ArgumentOutOfRangeException(nameof(buffs));
        }

        writer.Write(buff.Type);
        writer.Write(buff.Duration);
      }

      writer.Write((ushort)0);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.NpcBuffs,
      payload.ToArray()));
  }

  public static byte[] EncodeNpcHome(NpcHomeSnapshot snapshot)
  {
    return EncodeNpcHome(snapshot, snapshot.IsHomeless ? (byte)1 : (byte)0);
  }

  public static byte[] EncodeNpcHome(NpcHomeSnapshot snapshot, byte homeState)
  {
    if (snapshot.NpcId > short.MaxValue || snapshot.NpcId < short.MinValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.NpcId);
      writer.Write(snapshot.TileX);
      writer.Write(snapshot.TileY);
      writer.Write(homeState);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.NpcHome,
      payload.ToArray()));
  }

  public static byte[] EncodeProjectileReplication(ProjectileReplicationSnapshot snapshot)
  {
    return EncodeProjectileSync(ProjectileStateProjection.Project(snapshot));
  }

  public static byte[] EncodeProjectileSync(ProjectileSyncPacket packet)
  {
    byte flags = 0;
    if (packet.Ai0.HasValue)
    {
      flags |= 1 << 0;
    }

    if (packet.Ai1.HasValue)
    {
      flags |= 1 << 1;
    }

    if (packet.Ai2.HasValue)
    {
      flags |= 1 << 2;
    }

    if (packet.Banner.HasValue)
    {
      flags |= 1 << 3;
    }

    if (packet.Damage.HasValue)
    {
      flags |= 1 << 4;
    }

    if (packet.Knockback.HasValue)
    {
      flags |= 1 << 5;
    }

    if (packet.OriginalDamage.HasValue)
    {
      flags |= 1 << 6;
    }

    if (packet.Uuid.HasValue)
    {
      flags |= 1 << 7;
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.Identity);
      writer.Write(TerrariaWorldCoordinates.ToPixels(packet.Position.X));
      writer.Write(TerrariaWorldCoordinates.ToPixels(packet.Position.Y));
      writer.Write(TerrariaWorldCoordinates.ToPixels(packet.Velocity.X));
      writer.Write(TerrariaWorldCoordinates.ToPixels(packet.Velocity.Y));
      writer.Write(packet.Owner);
      writer.Write(packet.ProjectileType);
      writer.Write(flags);
      if (packet.Ai2.HasValue)
      {
        writer.Write((byte)1);
      }

      if (packet.Ai0.HasValue)
      {
        writer.Write(packet.Ai0.Value);
      }

      if (packet.Ai1.HasValue)
      {
        writer.Write(packet.Ai1.Value);
      }

      if (packet.Banner.HasValue)
      {
        writer.Write(packet.Banner.Value);
      }

      if (packet.Damage.HasValue)
      {
        writer.Write(packet.Damage.Value);
      }

      if (packet.Knockback.HasValue)
      {
        writer.Write(packet.Knockback.Value);
      }

      if (packet.OriginalDamage.HasValue)
      {
        writer.Write(packet.OriginalDamage.Value);
      }

      if (packet.Uuid.HasValue)
      {
        writer.Write(packet.Uuid.Value);
      }

      if (packet.Ai2.HasValue)
      {
        writer.Write(packet.Ai2.Value);
      }
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncProjectile,
      payload.ToArray()));
  }

  public static byte[] EncodeProjectileDespawn(ProjectileReplicationSnapshot snapshot)
  {
    int identity = snapshot.Identity == 0 ? snapshot.ReplicationId : snapshot.Identity;
    if (snapshot.IsActive || identity > short.MaxValue ||
        identity < short.MinValue || snapshot.Owner.Value < byte.MinValue ||
        snapshot.Owner.Value > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)identity);
      writer.Write((byte)snapshot.Owner.Value);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.KillProjectile,
      payload.ToArray()));
  }

  public static byte[] EncodeItemReplication(ItemReplicationSnapshot snapshot)
  {
    snapshot.Validate();
    if (snapshot.ReplicationId < short.MinValue || snapshot.ReplicationId > short.MaxValue ||
        snapshot.Stack.Quantity < short.MinValue || snapshot.Stack.Quantity > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((short)snapshot.ReplicationId);
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Position.X));
      writer.Write(TerrariaWorldCoordinates.ToPixels(snapshot.Position.Y));
      writer.Write(0.0f);
      writer.Write(0.0f);
      writer.Write((short)(snapshot.IsActive ? snapshot.Stack.Quantity : 0));
      writer.Write((byte)0);
      writer.Write((byte)(snapshot.IsActive ? 1 : 0));
      writer.Write((short)(snapshot.IsActive ? snapshot.Stack.ItemType : 0));
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncItem,
      payload.ToArray()));
  }

  public static byte[] EncodeRequestWorldData()
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.RequestWorldData,
      Array.Empty<byte>()));
  }

  public static byte[] EncodeSpawnTileData(SpawnTileDataRequestPacket packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.SpawnX);
      writer.Write(packet.SpawnY);
      writer.Write(packet.Team);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SpawnTileData,
      payload.ToArray()));
  }

  public static byte[] Encode(WorldDataPacket packet)
  {
    if (packet.Payload.Length == 0)
    {
      throw new InvalidDataException("Terraria WorldData payload cannot be empty.");
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.WorldData, packet.Payload));
  }

  public static byte[] Encode(WorldSectionSnapshot snapshot)
  {
    return Encode(snapshot, []);
  }

  public static byte[] EncodeTileSquare(
    WorldGrid world,
    int x,
    int y,
    byte width,
    byte height,
    byte changeType)
  {
    return TerrariaV1456Compatibility.EncodeTileSquare(world, x, y, width, height, changeType);
  }

  public static byte[] EncodeExploitDestroyTileSquare(
    WorldGrid world,
    ExploitDestroyTileSquareIntent intent,
    byte changeType = 0)
  {
    if (intent.RemoteClient != -1)
    {
      throw new ArgumentOutOfRangeException(nameof(intent));
    }

    return EncodeTileSquare(world, intent.X, intent.Y, intent.Width, intent.Height, changeType);
  }

  public static byte[] EncodeTrainingDummyTileEntitySharing(
    TileEntityPersistentState entity)
  {
    ArgumentNullException.ThrowIfNull(entity);
    if (entity.IsOpaque || entity.Type != 0 || entity.Payload.Count != 2 ||
        entity.TileX < short.MinValue || entity.TileX > short.MaxValue ||
        entity.TileY < short.MinValue || entity.TileY > short.MaxValue)
    {
      throw new ArgumentException("Tile entity is not a typed TrainingDummy state.", nameof(entity));
    }

    using MemoryStream payload = new();
    using BinaryWriter writer = new(payload);
    writer.Write(entity.Id);
    writer.Write(true);
    writer.Write((byte)0);
    writer.Write(entity.Id);
    writer.Write((short)entity.TileX);
    writer.Write((short)entity.TileY);
    writer.Write(entity.Payload[0]);
    writer.Write(entity.Payload[1]);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileEntitySharing,
      payload.ToArray()));
  }

  public static byte[] EncodeTrainingDummyTileEntityRemoval(int entityId)
  {
    using MemoryStream payload = new();
    using BinaryWriter writer = new(payload);
    writer.Write(entityId);
    writer.Write(false);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileEntitySharing,
      payload.ToArray()));
  }

  public static byte[] EncodeTrainingDummyTileEntityPlacement(int tileX, int tileY)
  {
    if (tileX < short.MinValue || tileX > short.MaxValue ||
        tileY < short.MinValue || tileY > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tileX));
    }

    using MemoryStream payload = new();
    using BinaryWriter writer = new(payload);
    writer.Write((short)tileX);
    writer.Write((short)tileY);
    writer.Write((byte)0);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileEntityPlacement,
      payload.ToArray()));
  }

  public static byte[] EncodeServerTileManipulation(
    byte action,
    int x,
    int y,
    ushort tileType,
    byte style)
  {
    if (x < short.MinValue || x > short.MaxValue || y < short.MinValue || y > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(action);
      writer.Write((short)x);
      writer.Write((short)y);
      writer.Write(tileType);
      writer.Write(style);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileManipulation,
      payload.ToArray()));
  }

  public static byte[] EncodeLiquidNetModule(IReadOnlyList<WorldLiquidSnapshot> changes)
  {
    ArgumentNullException.ThrowIfNull(changes);
    if (changes.Count > ushort.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(changes));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((ushort)0);
      writer.Write((ushort)changes.Count);
      for (int index = 0; index < changes.Count; index++)
      {
        WorldLiquidSnapshot change = changes[index];
        if (change.X < 0 || change.X > ushort.MaxValue ||
            change.Y < 0 || change.Y > ushort.MaxValue)
        {
          throw new ArgumentOutOfRangeException(nameof(changes));
        }

        writer.Write((change.X << 16) | change.Y);
        writer.Write(change.Amount);
        writer.Write(change.Type);
      }
    }

    return EncodeNetModules(payload.ToArray());
  }

  public static byte[] Encode(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests)
  {
    return Encode(snapshot, chests, []);
  }

  public static byte[] Encode(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests,
    IReadOnlyList<SignReplicationSnapshot> signs)
  {
    return TerrariaV1456Compatibility.EncodeTileSection(snapshot, chests, signs, []);
  }

  public static byte[] Encode(
    WorldSectionSnapshot snapshot,
    IReadOnlyList<ChestSnapshot> chests,
    IReadOnlyList<SignReplicationSnapshot> signs,
    IReadOnlyList<LegacyTileEntity> tileEntities)
  {
    return TerrariaV1456Compatibility.EncodeTileSection(
      snapshot,
      chests,
      signs,
      tileEntities);
  }

  public static IReadOnlyList<byte[]> CreateInitialWorldStream()
  {
    int sectionCount = InitialSectionColumns * InitialSectionRows;
    List<byte[]> frames = new(sectionCount + 2)
    {
      EncodeStatusTextSize(sectionCount, "Receiving tile data")
    };
    for (int sectionY = SpawnSectionY - 1;
      sectionY < SpawnSectionY - 1 + InitialSectionRows;
      sectionY++)
    {
      for (int sectionX = SpawnSectionX - 2;
        sectionX < SpawnSectionX - 2 + InitialSectionColumns;
        sectionX++)
      {
        frames.Add(EncodeEmptyTileSection(sectionX * SectionWidth, sectionY * SectionHeight));
      }
    }

    byte[] initialSpawn = TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.InitialSpawn, Array.Empty<byte>()));
    frames.Add(initialSpawn);
    return frames;
  }

  public static IReadOnlyList<byte[]> CreateInitialWorldStream(
    IReadOnlyList<WorldSectionSnapshot> snapshots)
  {
    ArgumentNullException.ThrowIfNull(snapshots);

    List<byte[]> frames = new(snapshots.Count + 2)
    {
      EncodeStatusTextSize(snapshots.Count, "Receiving tile data")
    };
    for (int index = 0; index < snapshots.Count; index++)
    {
      frames.Add(Encode(snapshots[index]));
    }

    frames.Add(TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.InitialSpawn, Array.Empty<byte>())));
    return frames;
  }

  public static byte[] Encode(PlayerSpawnPacket packet)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(packet.PlayerSlot);
      writer.Write(packet.SpawnX);
      writer.Write(packet.SpawnY);
      writer.Write(packet.RespawnTimer);
      writer.Write(packet.PlayerVersusEnvironmentDeaths);
      writer.Write(packet.PlayerVersusPlayerDeaths);
      writer.Write(packet.Team);
      writer.Write(packet.SpawnContext);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.PlayerSpawn,
      payload.ToArray()));
  }

  public static byte[] EncodeFinishedConnectingToServer()
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.FinishedConnectingToServer,
      Array.Empty<byte>()));
  }

  public static byte[] EncodeInitialNetModules()
  {
    byte[] payload = [0, 0, 0, 0];
    return EncodeNetModules(payload);
  }

  public static IReadOnlyList<byte[]> CreateDefaultJoinStateNetModules()
  {
    const int CreativePowerCount = 15;
    List<byte[]> frames = new(1 + CreativePowerCount + 8);
    frames.Add(EncodeBannerFullState(
      new int[DefaultBannerTypeCount],
      new ushort[DefaultBannerTypeCount]));

    for (byte powerId = 0; powerId < CreativePowerCount; powerId++)
    {
      frames.Add(EncodeNetModules([9, 0, 0, powerId, 0, 2]));
    }

    frames.Add(EncodeNetModules([5, 0, 0, 0, 0]));
    byte[] creativeStateFive = new byte[37];
    creativeStateFive[0] = 5;
    creativeStateFive[2] = 5;
    frames.Add(EncodeNetModules(creativeStateFive));
    frames.Add(EncodeNetModules([5, 0, 8, 0, 0, 0, 0, 0]));
    frames.Add(EncodeNetModules([5, 0, 9, 0, 0]));
    frames.Add(EncodeNetModules([5, 0, 10, 0, 0]));
    byte[] creativeStateEleven = new byte[37];
    creativeStateEleven[0] = 5;
    creativeStateEleven[2] = 11;
    Array.Fill(creativeStateEleven, byte.MaxValue, 5, 31);
    creativeStateEleven[36] = 127;
    frames.Add(EncodeNetModules(creativeStateEleven));
    frames.Add(EncodeNetModules([5, 0, 12, 0, 0, 0, 0, 0]));
    frames.Add(EncodeNetModules([5, 0, 13, 0, 0]));
    return frames;
  }

  public static IReadOnlyList<byte[]> CreateJoinGreetingNetModules(string playerName)
  {
    return TerrariaV1456Compatibility.CreateJoinGreetingFrames(
      LegacyWorldDataContext.CreateDomeDefaults(),
      playerName);
  }

  private static byte[] EncodeNetModules(ReadOnlySpan<byte> payload)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.NetModules,
      payload.ToArray()));
  }

  public static byte[] EncodePing()
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.Ping,
      Array.Empty<byte>()));
  }

  public static byte[] EncodeInvasionProgressReport(InvasionProgressReportPacket packet)
  {
    byte[] payload = new byte[sizeof(int) + sizeof(int) + sizeof(sbyte) + sizeof(sbyte)];
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0, sizeof(int)), packet.InvasionType);
    BinaryPrimitives.WriteInt32LittleEndian(
      payload.AsSpan(sizeof(int), sizeof(int)),
      packet.InvasionSize);
    payload[sizeof(int) + sizeof(int)] = unchecked((byte)packet.Progress);
    payload[sizeof(int) + sizeof(int) + sizeof(sbyte)] = unchecked((byte)packet.Wave);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.InvasionProgressReport,
      payload));
  }

  public static byte[] Encode(SyncPlayerChestLocationPacket packet)
  {
    byte[] payload = new byte[6];
    payload[0] = packet.PlayerId;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(byte), sizeof(short)), packet.X);
    BinaryPrimitives.WriteInt16LittleEndian(
      payload.AsSpan(sizeof(byte) + sizeof(short), sizeof(short)), packet.Y);
    payload[sizeof(byte) + sizeof(short) * 2] = packet.Type;
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncPlayerChestLocation,
      payload));
  }

  public static byte[] Encode(SyncRevengeMarkerPacket packet)
  {
    byte[] payload = new byte[sizeof(short)];
    BinaryPrimitives.WriteInt16LittleEndian(payload, packet.Id);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncRevengeMarker,
      payload));
  }

  public static byte[] Encode(RemoveRevengeMarkerPacket packet)
  {
    byte[] payload = new byte[sizeof(int)];
    BinaryPrimitives.WriteInt32LittleEndian(payload, packet.UniqueId);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.RemoveRevengeMarker,
      payload));
  }

  public static byte[] Encode(LandGolfBallInCupPacket packet)
  {
    byte[] payload = new byte[sizeof(byte) + sizeof(ushort) * 4];
    payload[0] = packet.PlayerId;
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(sizeof(byte), sizeof(ushort)), packet.X);
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(sizeof(byte) + sizeof(ushort), sizeof(ushort)), packet.Y);
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(sizeof(byte) + sizeof(ushort) * 2, sizeof(ushort)), packet.Value1);
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(sizeof(byte) + sizeof(ushort) * 3, sizeof(ushort)), packet.Value2);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.LandGolfBallInCup, payload));
  }

  public static byte[] Encode(FishOutNpcPacket packet)
  {
    byte[] payload = new byte[sizeof(ushort) * 2 + sizeof(short)];
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(0, sizeof(ushort)), packet.X);
    BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(sizeof(ushort), sizeof(ushort)), packet.Y);
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(ushort) * 2, sizeof(short)), packet.NpcId);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.FishOutNpc, payload));
  }

  public static InvasionProgressReportPacket DecodeInvasionProgressReport(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(int) + sizeof(int) + sizeof(sbyte) + sizeof(sbyte);
    if (frame.MessageId != TerrariaMessageId.InvasionProgressReport ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException(
        "Terraria InvasionProgressReport packet must contain a ten-byte payload.");
    }

    return new InvasionProgressReportPacket(
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span[..sizeof(int)]),
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(sizeof(int), sizeof(int))),
      unchecked((sbyte)frame.Payload.Span[sizeof(int) + sizeof(int)]),
      unchecked((sbyte)frame.Payload.Span[sizeof(int) + sizeof(int) + sizeof(sbyte)]));
  }

  public static byte[] EncodeSyncPlayerChestIndex(SyncPlayerChestIndexPacket packet)
  {
    byte[] payload = new byte[sizeof(byte) + sizeof(short)];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(sizeof(byte), sizeof(short)),
      packet.ChestIndex);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.SyncPlayerChestIndex,
      payload));
  }

  public static SyncPlayerChestIndexPacket DecodeSyncPlayerChestIndex(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(byte) + sizeof(short);
    if (frame.MessageId != TerrariaMessageId.SyncPlayerChestIndex ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException(
        "Terraria SyncPlayerChestIndex packet must contain a three-byte payload.");
    }

    return new SyncPlayerChestIndexPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span.Slice(sizeof(byte), sizeof(short))));
  }

  public static byte[] EncodeCombatTextInt(CombatTextIntPacket packet)
  {
    const int payloadLength = sizeof(float) + sizeof(float) + 3 + sizeof(int);
    byte[] payload = new byte[payloadLength];
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(0, sizeof(float)),
      BitConverter.SingleToInt32Bits(packet.PositionX));
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(sizeof(float), sizeof(float)),
      BitConverter.SingleToInt32Bits(packet.PositionY));
    payload[8] = packet.Color.Red;
    payload[9] = packet.Color.Green;
    payload[10] = packet.Color.Blue;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(11, sizeof(int)), packet.Text);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.CombatTextInt, payload));
  }

  public static CombatTextIntPacket DecodeCombatTextInt(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(float) + sizeof(float) + 3 + sizeof(int);
    if (frame.MessageId != TerrariaMessageId.CombatTextInt ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException("Terraria CombatTextInt packet must contain a fifteen-byte payload.");
    }

    return new CombatTextIntPacket(
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span[..sizeof(float)])),
      BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span.Slice(sizeof(float), sizeof(float)))),
      new TerrariaColor(frame.Payload.Span[8], frame.Payload.Span[9], frame.Payload.Span[10]),
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(11, sizeof(int))));
  }

  public static byte[] EncodeTemporaryAnimation(TemporaryAnimationPacket packet)
  {
    const int payloadLength = sizeof(short) + sizeof(ushort) + sizeof(short) + sizeof(short);
    byte[] payload = new byte[payloadLength];
    BinaryPrimitives.WriteInt16LittleEndian(payload.AsSpan(0, sizeof(short)), packet.AnimationType);
    BinaryPrimitives.WriteUInt16LittleEndian(
      payload.AsSpan(sizeof(short), sizeof(ushort)),
      packet.TileType);
    BinaryPrimitives.WriteInt16LittleEndian(
      payload.AsSpan(sizeof(short) + sizeof(ushort), sizeof(short)),
      packet.TileX);
    BinaryPrimitives.WriteInt16LittleEndian(
      payload.AsSpan(sizeof(short) + sizeof(ushort) + sizeof(short), sizeof(short)),
      packet.TileY);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.TemporaryAnimation, payload));
  }

  public static TemporaryAnimationPacket DecodeTemporaryAnimation(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(short) + sizeof(ushort) + sizeof(short) + sizeof(short);
    if (frame.MessageId != TerrariaMessageId.TemporaryAnimation ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException("Terraria TemporaryAnimation packet must contain an eight-byte payload.");
    }

    return new TemporaryAnimationPacket(
      BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span[..sizeof(short)]),
      BinaryPrimitives.ReadUInt16LittleEndian(
        frame.Payload.Span.Slice(sizeof(short), sizeof(ushort))),
      BinaryPrimitives.ReadInt16LittleEndian(
        frame.Payload.Span.Slice(sizeof(short) + sizeof(ushort), sizeof(short))),
      BinaryPrimitives.ReadInt16LittleEndian(
        frame.Payload.Span.Slice(sizeof(short) + sizeof(ushort) + sizeof(short), sizeof(short))));
  }

  public static byte[] EncodeQuestsCountSync(QuestsCountSyncPacket packet)
  {
    const int payloadLength = sizeof(byte) + sizeof(int) + sizeof(int);
    byte[] payload = new byte[payloadLength];
    payload[0] = packet.PlayerSlot;
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1, sizeof(int)),
      packet.AnglerQuestsFinished);
    BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(1 + sizeof(int), sizeof(int)),
      packet.GolferScoreAccumulated);
    return TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.QuestsCountSync, payload));
  }

  public static QuestsCountSyncPacket DecodeQuestsCountSync(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    const int payloadLength = sizeof(byte) + sizeof(int) + sizeof(int);
    if (frame.MessageId != TerrariaMessageId.QuestsCountSync ||
        frame.Payload.Length != payloadLength)
    {
      throw new InvalidDataException("Terraria QuestsCountSync packet must contain a nine-byte payload.");
    }

    return new QuestsCountSyncPacket(
      frame.Payload.Span[0],
      BinaryPrimitives.ReadInt32LittleEndian(frame.Payload.Span.Slice(1, sizeof(int))),
      BinaryPrimitives.ReadInt32LittleEndian(
        frame.Payload.Span.Slice(1 + sizeof(int), sizeof(int))));
  }

  public static byte[] EncodeAnglerQuestFinished(AnglerQuestFinishedPacket packet)
  {
    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.AnglerQuestFinished,
      Array.Empty<byte>()));
  }

  public static AnglerQuestFinishedPacket DecodeAnglerQuestFinished(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.AnglerQuestFinished || !frame.Payload.IsEmpty)
    {
      throw new InvalidDataException(
        "Terraria AnglerQuestFinished packet must contain an empty payload.");
    }

    return new AnglerQuestFinishedPacket();
  }

  public static byte[] EncodeRequestTeleportationByServer(
    RequestTeleportationByServerPacket packet)
  {
    if (!packet.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.RequestTeleportationByServer,
      new[] { packet.Selector }));
  }

  public static RequestTeleportationByServerPacket DecodeRequestTeleportationByServer(
    ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.RequestTeleportationByServer ||
        frame.Payload.Length != 1 || frame.Payload.Span[0] > 4)
    {
      throw new InvalidDataException(
        "Terraria RequestTeleportationByServer packet must contain selector 0 through 4.");
    }

    return new RequestTeleportationByServerPacket(frame.Payload.Span[0]);
  }

  public static void ValidatePing(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.Ping || !frame.Payload.IsEmpty)
    {
      throw new InvalidDataException("Terraria Ping packet must have an empty payload.");
    }
  }

  public static void ValidateClientSyncedInventory(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.ClientSyncedInventory || !frame.Payload.IsEmpty)
    {
      throw new InvalidDataException(
        "Terraria ClientSyncedInventory packet must have an empty payload.");
    }
  }

  public static short DecodeUniqueTownNpcInfoSyncRequest(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.UniqueTownNpcInfoSyncRequest ||
        frame.Payload.Length != sizeof(short))
    {
      throw new InvalidDataException(
        "Terraria UniqueTownNPCInfoSyncRequest packet must have a two-byte payload.");
    }

    return BinaryPrimitives.ReadInt16LittleEndian(frame.Payload.Span);
  }

  public static ClientTalkNpcPacket DecodeClientTalkNpc(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncTalkNpc ||
        frame.Payload.Length != RequiredClientTalkNpcLength)
    {
      throw new InvalidDataException("Terraria SyncTalkNPC packet must have a three-byte payload.");
    }

    ReadOnlySpan<byte> payload = frame.Payload.Span;
    return new ClientTalkNpcPacket(
      payload[0],
      BinaryPrimitives.ReadInt16LittleEndian(payload[sizeof(byte)..]));
  }

  public static byte DecodeClientTalkNpcPlayerSlot(ReadOnlySpan<byte> frameBytes)
  {
    return DecodeClientTalkNpc(frameBytes).PlayerSlot;
  }

  public static byte[] Encode(SetUserSlotPacket packet)
  {
    byte[] payload = [packet.PlayerSlot, packet.IsServerSideCharacter ? (byte)1 : (byte)0];
    TerrariaFrame frame = new(TerrariaMessageId.SetUserSlot, payload);
    return TerrariaFrameCodec.Encode(frame);
  }

  private static TerrariaColor ReadColor(BinaryReader reader)
  {
    return new TerrariaColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
  }

  private static void WriteColor(BinaryWriter writer, TerrariaColor color)
  {
    writer.Write(color.Red);
    writer.Write(color.Green);
    writer.Write(color.Blue);
  }

  private static void WriteNetworkText(BinaryWriter writer, LegacyNetworkText text)
  {
    writer.Write((byte)text.Mode);
    writer.Write(text.Text);
    if (text.Mode == LegacyNetworkTextMode.Literal)
    {
      return;
    }

    int serializedSubstitutionCount = text.Substitutions.Count & byte.MaxValue;
    writer.Write((byte)serializedSubstitutionCount);
    for (int index = 0; index < serializedSubstitutionCount; index++)
    {
      WriteNetworkText(writer, text.Substitutions[index]);
    }
  }

  private static LegacyNetworkText ReadNetworkText(BinaryReader reader)
  {
    LegacyNetworkTextMode mode = (LegacyNetworkTextMode)reader.ReadByte();
    string text = reader.ReadString();
    switch (mode)
    {
      case LegacyNetworkTextMode.Literal:
        return LegacyNetworkText.Literal(text);
      case LegacyNetworkTextMode.Formattable:
      case LegacyNetworkTextMode.LocalizationKey:
      {
        int substitutionCount = reader.ReadByte();
        LegacyNetworkText[] substitutions = new LegacyNetworkText[substitutionCount];
        for (int index = 0; index < substitutions.Length; index++)
        {
          substitutions[index] = ReadNetworkText(reader);
        }

        return mode == LegacyNetworkTextMode.Formattable
          ? LegacyNetworkText.Formattable(text, substitutions)
          : LegacyNetworkText.LocalizationKey(text, substitutions);
      }
      default:
        throw new InvalidDataException("Terraria NetworkText mode is unsupported.");
    }
  }

  private static byte[] EncodeBannerFullState(
    IReadOnlyList<int> killCounts,
    IReadOnlyList<ushort> claimableCounts)
  {
    if (killCounts.Count > short.MaxValue || claimableCounts.Count > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(killCounts));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write((ushort)BannerModuleId);
      writer.Write((byte)BannerModuleMessageType.FullState);
      writer.Write((short)killCounts.Count);
      for (int index = 0; index < killCounts.Count; index++)
      {
        writer.Write(killCounts[index]);
      }

      writer.Write((short)claimableCounts.Count);
      for (int index = 0; index < claimableCounts.Count; index++)
      {
        writer.Write(claimableCounts[index]);
      }
    }

    return EncodeNetModules(payload.ToArray());
  }

  private static int[] ReadBannerKillCounts(BinaryReader reader, MemoryStream stream)
  {
    RequireBannerPayloadAtLeast(stream, sizeof(short));
    short serializedCount = reader.ReadInt16();
    if (serializedCount < 0)
    {
      throw new InvalidDataException("Terraria Banners module has a negative kill-count length.");
    }

    int count = serializedCount;
    RequireBannerPayloadAtLeast(stream, count * sizeof(int) + sizeof(short));
    int[] killCounts = new int[count];
    for (int index = 0; index < killCounts.Length; index++)
    {
      killCounts[index] = reader.ReadInt32();
    }

    return killCounts;
  }

  private static ushort[] ReadBannerClaimableCounts(BinaryReader reader, MemoryStream stream)
  {
    RequireBannerPayloadAtLeast(stream, sizeof(short));
    short serializedCount = reader.ReadInt16();
    if (serializedCount < 0)
    {
      throw new InvalidDataException("Terraria Banners module has a negative claim-count length.");
    }

    int count = serializedCount;
    RequireBannerPayloadLength(stream, count * sizeof(ushort));
    ushort[] claimableCounts = new ushort[count];
    for (int index = 0; index < claimableCounts.Length; index++)
    {
      claimableCounts[index] = reader.ReadUInt16();
    }

    return claimableCounts;
  }

  private static void RequireBannerPayloadAtLeast(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position < requiredLength)
    {
      throw new InvalidDataException("Terraria Banners module has a truncated payload.");
    }
  }

  private static void RequireBannerPayloadEnd(MemoryStream stream)
  {
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("Terraria Banners module contains trailing payload bytes.");
    }
  }

  private static void RequireBannerPayloadLength(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position != requiredLength)
    {
      throw new InvalidDataException("Terraria Banners module has an invalid payload length.");
    }
  }

  private static int ReadCraftingCount(BinaryReader reader, MemoryStream stream, string name)
  {
    int count = ReadCraftingInt(reader, stream, $"{name} count");
    if (count < 0 || count > stream.Length - stream.Position)
    {
      throw new InvalidDataException($"Terraria crafting {name} count is invalid.");
    }

    return count;
  }

  private static int ReadCraftingInt(BinaryReader reader, MemoryStream stream, string fieldName)
  {
    RequireCraftingPayloadAtLeast(stream, sizeof(byte));
    try
    {
      return reader.Read7BitEncodedInt();
    }
    catch (EndOfStreamException exception)
    {
      throw new InvalidDataException(
        $"Terraria crafting {fieldName} is truncated.",
        exception);
    }
    catch (FormatException exception)
    {
      throw new InvalidDataException(
        $"Terraria crafting {fieldName} has an invalid 7-bit encoding.",
        exception);
    }
  }

  private static void RequireCraftingPayloadAtLeast(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position < requiredLength)
    {
      throw new InvalidDataException("Terraria crafting module has a truncated payload.");
    }
  }

  private static IReadOnlyList<TagEffectSparseEntry> ReadTagEffectSparseArray(
    BinaryReader reader,
    MemoryStream stream)
  {
    List<TagEffectSparseEntry> entries = new();
    while (true)
    {
      RequireTagEffectPayloadAtLeast(stream, sizeof(byte));
      byte npcIndex = reader.ReadByte();
      if (npcIndex >= MaximumTagEffectNpcCount)
      {
        return entries;
      }

      RequireTagEffectPayloadAtLeast(stream, sizeof(int));
      entries.Add(new TagEffectSparseEntry(npcIndex, reader.ReadInt32()));
    }
  }

  private static void RequireTagEffectPayloadAtLeast(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position < requiredLength)
    {
      throw new InvalidDataException("Terraria tag-effect module has a truncated payload.");
    }
  }

  private static void RequireTagEffectPayloadEnd(MemoryStream stream)
  {
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException(
        "Terraria tag-effect module contains trailing payload bytes.");
    }
  }

  private static void RequireTagEffectPayloadLength(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position != requiredLength)
    {
      throw new InvalidDataException("Terraria tag-effect module has an invalid payload length.");
    }
  }

  private static LeashedKiteStatePacket ReadLeashedKiteState(
    BinaryReader reader,
    MemoryStream stream,
    bool full)
  {
    int? projectileType = null;
    if (full)
    {
      projectileType = ReadLeashedEntityInt(reader, stream, "kite projectile type");
    }

    RequireLeashedEntityPayloadAtLeast(
      stream,
      sizeof(float) * 5 + sizeof(uint) + sizeof(byte));
    return new LeashedKiteStatePacket(
      projectileType,
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadUInt32(),
      reader.ReadByte(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle());
  }

  private static LeashedCritterStatePacket ReadLeashedCritterState(
    BinaryReader reader,
    MemoryStream stream,
    int entityType,
    bool full)
  {
    int? npcType = null;
    float? width = null;
    float? height = null;
    if (full)
    {
      npcType = ReadLeashedEntityInt(reader, stream, "critter NPC type");
      RequireLeashedEntityPayloadAtLeast(stream, sizeof(float) * 2);
      width = reader.ReadSingle();
      height = reader.ReadSingle();
    }

    const int CritterStateLength =
      sizeof(uint) + sizeof(byte) + sizeof(uint) + sizeof(short) + sizeof(byte) +
      sizeof(sbyte) * 2;
    RequireLeashedEntityPayloadAtLeast(stream, CritterStateLength);
    uint packedPositionOffset = reader.ReadUInt32();
    bool facingRight = reader.ReadBoolean();
    uint randomState = reader.ReadUInt32();
    short waitTime = reader.ReadInt16();
    byte state = reader.ReadByte();
    sbyte targetOffsetX = reader.ReadSByte();
    sbyte targetOffsetY = reader.ReadSByte();
    LeashedCritterExtensionKind extensionKind = GetLeashedCritterExtensionKind(entityType);
    byte? extensionValue = null;
    if (full && extensionKind != LeashedCritterExtensionKind.None)
    {
      RequireLeashedEntityPayloadAtLeast(stream, sizeof(byte));
      extensionValue = reader.ReadByte();
    }

    return new LeashedCritterStatePacket(
      npcType,
      width,
      height,
      packedPositionOffset,
      facingRight,
      randomState,
      waitTime,
      state,
      targetOffsetX,
      targetOffsetY,
      extensionKind,
      extensionValue);
  }

  private static LeashedCritterExtensionKind GetLeashedCritterExtensionKind(int entityType)
  {
    return entityType switch
    {
      7 => LeashedCritterExtensionKind.ButterflyVariant,
      11 => LeashedCritterExtensionKind.ShimmerFlyOldPositionsLength,
      _ => LeashedCritterExtensionKind.None
    };
  }

  private static int ReadLeashedEntityInt(
    BinaryReader reader,
    MemoryStream stream,
    string fieldName)
  {
    RequireLeashedEntityPayloadAtLeast(stream, sizeof(byte));
    try
    {
      return reader.Read7BitEncodedInt();
    }
    catch (EndOfStreamException exception)
    {
      throw new InvalidDataException(
        $"Terraria leashed-entity {fieldName} is truncated.",
        exception);
    }
    catch (FormatException exception)
    {
      throw new InvalidDataException(
        $"Terraria leashed-entity {fieldName} has an invalid 7-bit encoding.",
        exception);
    }
  }

  private static void RequireLeashedEntityPayloadAtLeast(MemoryStream stream, int requiredLength)
  {
    if (stream.Length - stream.Position < requiredLength)
    {
      throw new InvalidDataException("Terraria leashed-entity module has a truncated payload.");
    }
  }

  private static void RequireLeashedEntityPayloadEnd(MemoryStream stream)
  {
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException(
        "Terraria leashed-entity module contains trailing payload bytes.");
    }
  }

  private static CreativePowerPayloadKind GetCreativePowerPayloadKind(ushort powerId)
  {
    return powerId switch
    {
      0 => CreativePowerPayloadKind.SharedToggle,
      1 or 2 or 3 or 4 => CreativePowerPayloadKind.SharedButton,
      5 or 11 => CreativePowerPayloadKind.PerPlayerToggle,
      6 or 7 or 8 or 12 => CreativePowerPayloadKind.SharedSlider,
      9 or 10 or 13 => CreativePowerPayloadKind.SharedToggle,
      14 => CreativePowerPayloadKind.PerPlayerSlider,
      _ => throw new InvalidDataException(
        $"Terraria CreativePower id {powerId} is not registered in V1456.")
    };
  }

  private static void RequireCreativePowerPayloadEnd(MemoryStream stream, ushort powerId)
  {
    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException(
        $"Terraria CreativePower {powerId} contains trailing payload bytes.");
    }
  }

  private static void RequireCreativePowerPayloadLength(
    MemoryStream stream,
    ushort powerId,
    int requiredLength)
  {
    if (stream.Length - stream.Position != requiredLength)
    {
      throw new InvalidDataException(
        $"Terraria CreativePower {powerId} has an invalid payload length.");
    }
  }

  private static void RequireCreativePowerPayloadAtLeast(
    MemoryStream stream,
    ushort powerId,
    int requiredLength)
  {
    if (stream.Length - stream.Position < requiredLength)
    {
      throw new InvalidDataException(
        $"Terraria CreativePower {powerId} has a truncated payload.");
    }
  }

  private static byte[] EncodeEmptyTileSection(int x, int y)
  {
    using MemoryStream compressedPayload = new();
    using (DeflateStream compressor = new(
      compressedPayload,
      CompressionMode.Compress,
      leaveOpen: true))
    using (BinaryWriter writer = new(compressor, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(x);
      writer.Write(y);
      writer.Write(SectionWidth);
      writer.Write(SectionHeight);
      writer.Write((byte)0x80);
      writer.Write((short)(SectionWidth * SectionHeight - 1));
      writer.Write((short)0);
      writer.Write((short)0);
      writer.Write((short)0);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.TileSection,
      compressedPayload.ToArray()));
  }

  public static byte[] EncodeStatusTextSize(int sectionCount, string text)
  {
    return EncodeStatusTextSize(
      sectionCount,
      LegacyNetworkText.Literal(text),
      statusTextFlags: 0);
  }

  public static byte[] EncodeStatusTextSize(
    int sectionCount,
    LegacyNetworkText text,
    byte statusTextFlags)
  {
    ArgumentNullException.ThrowIfNull(text);

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(sectionCount);
      WriteNetworkText(writer, text);
      writer.Write(statusTextFlags);
    }

    return TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.StatusTextSize,
      payload.ToArray()));
  }

  internal static byte[] CreateDefaultWorldDataPayload()
  {
    return TerrariaV1456Compatibility.CreateWorldDataPayload(
      LegacyWorldDataContext.CreateDomeDefaults());
  }

  private static PlayerVitalsPacket DecodePlayerVitals(
    ReadOnlySpan<byte> frameBytes,
    TerrariaMessageId expectedMessageId,
    string packetName)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != expectedMessageId || frame.Payload.Length != RequiredVitalsLength)
    {
      throw new InvalidDataException($"Terraria frame is not a valid {packetName} packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: false);
    return new PlayerVitalsPacket(reader.ReadByte(), reader.ReadInt16(), reader.ReadInt16());
  }

}
