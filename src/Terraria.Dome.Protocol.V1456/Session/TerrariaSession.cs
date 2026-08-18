using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Session;

public sealed class TerrariaSession
{
  private const int PlayerItemSlotCount = 990;
  private readonly byte _assignedPlayerSlot;
  private readonly List<ushort> _buffTypes = new();
  private readonly PlayerEquipmentPacket[] _equipment = CreateEmptyEquipment();
  private PlayerBootstrapState? _bootstrap;
  private PlayerVitalsPacket? _life;
  private PlayerLoadoutPacket? _loadout;
  private PlayerVitalsPacket? _mana;
  private PlayerProfilePacket? _profile;
  private string? _uuid;

  public TerrariaSession(byte assignedPlayerSlot)
  {
    _assignedPlayerSlot = assignedPlayerSlot;
  }

  public TerrariaSessionState State { get; private set; }

  public LegacyPlayerControlsState? LegacyPlayerControls { get; private set; }

  public byte[] AcceptHello(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Connected)
    {
      throw new InvalidDataException("Terraria Hello is not valid in the current session state.");
    }

    HelloPacket hello = TerrariaPacketCodec.DecodeHello(frameBytes);
    if (!string.Equals(
      hello.ProtocolIdentifier,
      TerrariaProtocolVersion.HelloIdentifier,
      StringComparison.Ordinal))
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria client protocol identifier is incompatible.");
    }

    State = TerrariaSessionState.UserSlotAssigned;
    return TerrariaPacketCodec.Encode(new SetUserSlotPacket(
      _assignedPlayerSlot,
      IsServerSideCharacter: false));
  }

  public PlayerProfilePacket AcceptPlayerProfile(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.UserSlotAssigned)
    {
      throw new InvalidDataException("Terraria SyncPlayer is not valid in the current session state.");
    }

    PlayerProfilePacket profile = TerrariaPacketCodec.DecodePlayerProfile(frameBytes);
    if (profile.PlayerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria SyncPlayer is not owned by this session.");
    }

    State = TerrariaSessionState.PlayerProfileReceived;
    _profile = profile;
    return profile;
  }

  public PlayerEquipmentPacket AcceptPlayerEquipment(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("SyncEquipment");
    PlayerEquipmentPacket equipment = TerrariaPacketCodec.DecodePlayerEquipment(frameBytes);
    EnsurePlayerOwnership(equipment.PlayerSlot, "SyncEquipment");
    _equipment[equipment.SlotId] = equipment;
    return equipment;
  }

  public PlayerEquipmentPacket AcceptActivePlayerEquipment(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("SyncEquipment");
    PlayerEquipmentPacket equipment = TerrariaPacketCodec.DecodePlayerEquipment(frameBytes);
    EnsurePlayerOwnership(equipment.PlayerSlot, "SyncEquipment");
    return equipment;
  }

  public PlayerBuffsPacket AcceptPlayerBuffs(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("PlayerBuffs");
    PlayerBuffsPacket buffs = TerrariaPacketCodec.DecodePlayerBuffs(frameBytes);
    EnsurePlayerOwnership(buffs.PlayerSlot, "PlayerBuffs");
    _buffTypes.Clear();
    _buffTypes.AddRange(buffs.BuffTypes);
    return buffs;
  }

  public PlayerBuffsPacket AcceptActivePlayerBuffs(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("PlayerBuffs");
    PlayerBuffsPacket buffs = TerrariaPacketCodec.DecodePlayerBuffs(frameBytes);
    EnsurePlayerOwnership(buffs.PlayerSlot, "PlayerBuffs");
    return buffs;
  }

  public void AcceptClientProjectile(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("SyncProjectile");
    byte playerSlot = TerrariaPacketCodec.DecodeClientProjectileOwner(frameBytes);
    EnsurePlayerOwnership(playerSlot, "SyncProjectile");
  }

  public void AcceptClientSyncedInventory(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("ClientSyncedInventory");
    TerrariaPacketCodec.ValidateClientSyncedInventory(frameBytes);
  }

  public void AcceptUniqueTownNpcInfoSyncRequest(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("UniqueTownNPCInfoSyncRequest");

    // The legacy server consumes this request and ignores unavailable NPC slots.
    _ = TerrariaPacketCodec.DecodeUniqueTownNpcInfoSyncRequest(frameBytes);
  }

  public void AcceptClientTalkNpc(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("SyncTalkNPC");
    ClientTalkNpcPacket talkNpc = TerrariaPacketCodec.DecodeClientTalkNpc(frameBytes);
    EnsurePlayerOwnership(talkNpc.PlayerSlot, "SyncTalkNPC");

    // Dome has no NPC conversation authority, but V1456 TalkNpc is fully consumed.
    _ = talkNpc.TalkNpc;
  }

  public ClientProjectileTermination AcceptClientProjectileTermination(
    ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("KillProjectile");
    ClientProjectileTermination termination = TerrariaPacketCodec.DecodeClientProjectileTermination(
      frameBytes);
    EnsurePlayerOwnership(termination.Owner, "KillProjectile");
    return termination;
  }

  public PlayerLoadoutPacket AcceptPlayerLoadout(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("SyncLoadout");
    PlayerLoadoutPacket loadout = TerrariaPacketCodec.DecodePlayerLoadout(frameBytes);
    EnsurePlayerOwnership(loadout.PlayerSlot, "SyncLoadout");
    _loadout = loadout;
    return loadout;
  }

  public PlayerVitalsPacket AcceptPlayerLifeMana(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("PlayerLifeMana");
    PlayerVitalsPacket life = TerrariaPacketCodec.DecodePlayerLifeMana(frameBytes);
    EnsurePlayerOwnership(life.PlayerSlot, "PlayerLifeMana");
    _life = life;
    return life;
  }

  public PlayerVitalsPacket AcceptActivePlayerLifeMana(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("PlayerLifeMana");
    PlayerVitalsPacket life = TerrariaPacketCodec.DecodePlayerLifeMana(frameBytes);
    EnsurePlayerOwnership(life.PlayerSlot, "PlayerLifeMana");
    return life;
  }

  public PlayerVitalsPacket AcceptPlayerMana(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("ItemRotationAndAnimation");
    PlayerVitalsPacket mana = TerrariaPacketCodec.DecodePlayerMana(frameBytes);
    EnsurePlayerOwnership(mana.PlayerSlot, "ItemRotationAndAnimation");
    _mana = mana;
    return mana;
  }

  public PlayerVitalsPacket AcceptActivePlayerMana(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("ItemRotationAndAnimation");
    PlayerVitalsPacket mana = TerrariaPacketCodec.DecodePlayerMana(frameBytes);
    EnsurePlayerOwnership(mana.PlayerSlot, "ItemRotationAndAnimation");
    return mana;
  }

  public PlayerUuidPacket AcceptPlayerUuid(ReadOnlySpan<byte> frameBytes)
  {
    EnsureBootstrapState("PlayerUuid");
    PlayerUuidPacket uuid = TerrariaPacketCodec.DecodePlayerUuid(frameBytes);
    _uuid = uuid.Value;
    return uuid;
  }

  public PlayerControlIntent AcceptPlayerControls(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria PlayerControls is not valid in the current session state.");
    }

    LegacyPlayerControlsProjection projection =
      TerrariaPacketCodec.DecodePlayerControlsCompatibility(frameBytes);
    PlayerControlIntent controls = projection.Intent;
    if (controls.PlayerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria PlayerControls is not owned by this session.");
    }

    LegacyPlayerControls = projection.State;
    State = TerrariaSessionState.Active;
    return controls;
  }

  public PlayerZonePacket AcceptPlayerZone(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("SyncPlayerZone");
    PlayerZonePacket zone = TerrariaPacketCodec.DecodePlayerZone(frameBytes);
    EnsurePlayerOwnership(zone.PlayerSlot, "SyncPlayerZone");
    return zone;
  }

  public NetModulePacket AcceptNetModule(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria NetModules is not valid in the current session state.");
    }

    NetModulePacket packet = TerrariaPacketCodec.DecodeNetModule(frameBytes);
    if (packet.ModuleId == 0)
    {
      _ = TerrariaPacketCodec.DecodeLiquidNetModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 1)
    {
      _ = TerrariaPacketCodec.DecodeClientTextModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 2)
    {
      _ = TerrariaPacketCodec.DecodeNetPingModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 3)
    {
      _ = TerrariaPacketCodec.DecodeNetAmbienceModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 4)
    {
      _ = TerrariaPacketCodec.DecodeBestiaryModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 6)
    {
      _ = TerrariaPacketCodec.DecodeCreativeUnlocksPlayerReportModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 7)
    {
      _ = TerrariaPacketCodec.DecodeTeleportPylonModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 8)
    {
      _ = TerrariaPacketCodec.DecodeParticleOrchestraModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 9)
    {
      _ = TerrariaPacketCodec.DecodeCreativePowerPermissionModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 10)
    {
      _ = TerrariaPacketCodec.DecodeBannerModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 11)
    {
      _ = TerrariaPacketCodec.DecodeCraftingRequestModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 12)
    {
      _ = TerrariaPacketCodec.DecodeTagEffectModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 13)
    {
      _ = TerrariaPacketCodec.DecodeLeashedEntityModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId == 14)
    {
      _ = TerrariaPacketCodec.DecodeUnbreakableWallScanModule(frameBytes);
      return packet;
    }

    if (packet.ModuleId != 5)
    {
      return packet;
    }

    CreativePowerModulePacket creativePower = TerrariaPacketCodec.DecodeCreativePowerModule(
      frameBytes);
    if (creativePower.PowerId == 14)
    {
      JourneySpawnRatePacket request = TerrariaPacketCodec.DecodeJourneySpawnRate(frameBytes);
      EnsurePlayerOwnership(request.PlayerSlot, "NetModules");
    }

    return packet;
  }

  public TileManipulationIntent AcceptTileManipulation(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException(
        "Terraria TileManipulation is not valid in the current session state.");
    }

    return TerrariaPacketCodec.DecodeTileManipulation(frameBytes);
  }

  public RequestSectionPacket AcceptRequestSection(ReadOnlySpan<byte> frameBytes)
  {
    EnsureActiveState("RequestSection");
    return TerrariaPacketCodec.DecodeRequestSection(frameBytes);
  }

  public ChestOpenIntent AcceptChestOpen(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria RequestChestOpen is not valid in the current session state.");
    }

    ChestOpenIntent intent = TerrariaPacketCodec.DecodeChestOpen(frameBytes);
    if (intent.PlayerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria RequestChestOpen is not owned by this session.");
    }

    return intent;
  }

  public DoorToggleIntent AcceptDoorToggle(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria ToggleDoorState is not valid in the current session state.");
    }

    DoorToggleIntent intent = TerrariaPacketCodec.DecodeDoorToggle(frameBytes);
    return intent;
  }

  public SignOpenRequestPacket AcceptSignOpenRequest(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria OpenSignRequest is not valid in the current session state.");
    }

    return TerrariaPacketCodec.DecodeSignOpenRequest(frameBytes);
  }

  public SignUpdateIntent AcceptSignUpdate(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria OpenSignResponse is not valid in the current session state.");
    }

    SignUpdateIntent intent = TerrariaPacketCodec.DecodeSignUpdate(frameBytes);

    // The source server replaces this client-reported slot with the socket identity.
    _ = intent.PlayerSlot;

    return intent;
  }

  public ChestTransferIntent AcceptChestTransfer(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria SyncPlayerChest is not valid in the current session state.");
    }

    ChestTransferIntent intent = TerrariaPacketCodec.DecodeChestTransfer(frameBytes);
    if (intent.PlayerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria SyncPlayerChest is not owned by this session.");
    }

    return intent;
  }

  public PlayerBootstrapState AcceptRequestWorldData(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.PlayerProfileReceived)
    {
      throw new InvalidDataException(
        "Terraria RequestWorldData is not valid in the current session state.");
    }

    _ = TerrariaPacketCodec.DecodeRequestWorldData(frameBytes);
    if (string.IsNullOrWhiteSpace(_uuid) || _profile is null)
    {
      throw new InvalidDataException("Terraria RequestWorldData requires a player UUID.");
    }

    _bootstrap = new PlayerBootstrapState(
      _uuid,
      _profile.Value,
      _life,
      _mana,
      _buffTypes,
      _loadout,
      _equipment);
    State = TerrariaSessionState.WorldDataRequested;
    return _bootstrap;
  }

  public SpawnTileDataRequestPacket AcceptSpawnTileData(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.WorldDataRequested)
    {
      throw new InvalidDataException("Terraria SpawnTileData is not valid in the current session state.");
    }

    SpawnTileDataRequestPacket request = TerrariaPacketCodec.DecodeSpawnTileData(frameBytes);
    State = TerrariaSessionState.TileDataRequested;
    return request;
  }

  public void MarkInitialWorldStreamSent()
  {
    if (State != TerrariaSessionState.TileDataRequested)
    {
      throw new InvalidDataException("Terraria initial world stream is not valid in the current session state.");
    }

    State = TerrariaSessionState.AwaitingPlayerSpawn;
  }

  public PlayerSpawnPacket AcceptPlayerSpawn(ReadOnlySpan<byte> frameBytes)
  {
    if (State != TerrariaSessionState.AwaitingPlayerSpawn &&
        State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException("Terraria PlayerSpawn is not valid in the current session state.");
    }

    PlayerSpawnPacket spawn = TerrariaPacketCodec.DecodePlayerSpawn(frameBytes);
    if (spawn.PlayerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException("Terraria PlayerSpawn is not owned by this session.");
    }

    State = TerrariaSessionState.Active;
    return spawn;
  }

  private static PlayerEquipmentPacket[] CreateEmptyEquipment()
  {
    PlayerEquipmentPacket[] equipment = new PlayerEquipmentPacket[PlayerItemSlotCount];
    for (int slotId = 0; slotId < equipment.Length; slotId++)
    {
      equipment[slotId] = new PlayerEquipmentPacket(
        0,
        slotId,
        0,
        0,
        0,
        IsFavorited: false,
        IsNewAndShiny: false);
    }

    return equipment;
  }

  private void EnsureBootstrapState(string packetName)
  {
    if (State != TerrariaSessionState.PlayerProfileReceived)
    {
      throw new InvalidDataException(
        $"Terraria {packetName} is not valid in the current session state.");
    }
  }

  private void EnsureActiveState(string packetName)
  {
    if (State != TerrariaSessionState.Active)
    {
      throw new InvalidDataException($"Terraria {packetName} is not valid in the current session state.");
    }
  }

  private void EnsurePlayerOwnership(byte playerSlot, string packetName)
  {
    if (playerSlot != _assignedPlayerSlot)
    {
      State = TerrariaSessionState.Closed;
      throw new InvalidDataException($"Terraria {packetName} is not owned by this session.");
    }
  }

}
