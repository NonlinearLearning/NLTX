using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Validates bounded player-state uploads and projects the authenticated sender slot.
/// </summary>
public sealed class PlayerAdmissionPacketHandlers :
    IPacketHandler<SyncPlayerPacket>,
    IPacketHandler<SyncEquipmentPacket>,
    IPacketHandler<PlayerLifeManaPacket>,
    IPacketHandler<Unknown42Packet>,
    IPacketHandler<PlayerBuffsPacket>,
    IPacketHandler<Unknown68Packet>,
    IPacketHandler<ClientSyncedInventoryPacket>,
    IPacketHandler<SyncLoadoutPacket>,
    IPacketHandler<TeamChangePacket>,
    IPacketHandler<SyncPlayerZonePacket>,
    IPacketHandler<SyncTalkNPCPacket>,
    IPacketHandler<ItemRotationAndAnimationPacket>,
    IPacketHandler<ItemUseSoundPacket> {
  private const int MaxPlayerNameLength = 20;
  private const int MaxInventorySlot = 989;
  private const int MaxItemStack = 9999;
  private const int MaxLifeOrMana = 30000;
  private const int MaxBuffCount = 44;
  private const int MaxNpcIndex = 199;

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncPlayerPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Name.Length > MaxPlayerNameLength || !float.IsFinite(packet.VoicePitchOffset)) {
      return Rejected("InvalidPlayerInfo");
    }

    var projection = new SyncPlayerPacket {
      Player = context.Actor.PlayerSlot,
      SkinVariant = packet.SkinVariant,
      VoiceVariant = packet.VoiceVariant,
      VoicePitchOffset = packet.VoicePitchOffset,
      Hair = packet.Hair,
      Name = packet.Name,
      HairDye = packet.HairDye,
      HiddenAccessories = packet.HiddenAccessories,
      HideMisc = packet.HideMisc,
      HairColor = packet.HairColor,
      SkinColor = packet.SkinColor,
      EyeColor = packet.EyeColor,
      ShirtColor = packet.ShirtColor,
      UnderShirtColor = packet.UnderShirtColor,
      PantsColor = packet.PantsColor,
      ShoeColor = packet.ShoeColor,
      DifficultyAndAccessoryFlags = packet.DifficultyAndAccessoryFlags,
      BiomeAndCartFlags = packet.BiomeAndCartFlags,
      PermanentUpgradeFlags = packet.PermanentUpgradeFlags
    };
    return Project(context, projection);
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncEquipmentPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Slot < 0 || packet.Slot > MaxInventorySlot) {
      return Rejected("InvalidEquipmentSlot");
    }
    if (packet.Stack < 0 || packet.Stack > MaxItemStack) {
      return Rejected("InvalidEquipmentStack");
    }
    if (packet.ItemType < -1) {
      return Rejected("InvalidEquipmentType");
    }
    return Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      PlayerLifeManaPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return packet.Life is < 0 or > MaxLifeOrMana || packet.MaximumLife is < 0 or > MaxLifeOrMana
        ? Rejected("InvalidPlayerLife")
        : Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown42Packet packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return packet.Mana is < 0 or > MaxLifeOrMana || packet.MaximumMana is < 0 or > MaxLifeOrMana
        ? Rejected("InvalidPlayerMana")
        : Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      PlayerBuffsPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return packet.BuffTypes.Count > MaxBuffCount
        ? Rejected("TooManyPlayerBuffs")
        : Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      Unknown68Packet packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return packet.ClientUuid.Length > 128
        ? Rejected("ClientUuidTooLong")
        : Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      ClientSyncedInventoryPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return packet.Bytes.Length != 0
        ? Rejected("InventorySyncMarkerMustBeEmpty")
        : Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncPlayerZonePacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    return Project(context, new SyncPlayerZonePacket {
      Player = context.Actor.PlayerSlot,
      Zone1 = packet.Zone1,
      Zone2 = packet.Zone2,
      Zone3 = packet.Zone3,
      Zone4 = packet.Zone4,
      Zone5 = packet.Zone5,
      TownNpcCount = packet.TownNpcCount
    });
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncTalkNPCPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.NpcIndex is < -1 or > MaxNpcIndex) {
      return Rejected("InvalidTalkNpcIndex");
    }
    return Project(context, new SyncTalkNPCPacket {
      Player = context.Actor.PlayerSlot,
      NpcIndex = packet.NpcIndex
    });
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      TeamChangePacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.Team > 5) {
      return Rejected("InvalidPlayerTeam");
    }
    return Project(context, new TeamChangePacket {
      Player = context.Actor.PlayerSlot,
      Team = packet.Team
    });
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      ItemRotationAndAnimationPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (!float.IsFinite(packet.ItemRotation) || packet.ItemAnimation < 0) {
      return Rejected("InvalidItemAnimation");
    }
    // Vanilla forwards this presentation notice to other players, excluding the sender.
    // The headless host has no animation simulation or presentation relay.
    return Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      ItemUseSoundPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    // Vanilla relays this sound to other players only. The headless host has no sound relay.
    return Accepted();
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncLoadoutPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.LoadoutIndex > 2) {
      return Rejected("InvalidLoadoutIndex");
    }
    var projection = new SyncLoadoutPacket {
      Player = context.Actor.PlayerSlot,
      LoadoutIndex = packet.LoadoutIndex,
      AccessoryVisibilityMask = packet.AccessoryVisibilityMask
    };
    return Project(context, projection);
  }

  private static ValueTask<PacketHandlingResult> Accepted() {
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  private static ValueTask<PacketHandlingResult> Rejected(string code) {
    return ValueTask.FromResult(new PacketHandlingResult(false, rejectionCode: code));
  }

  private static ValueTask<PacketHandlingResult> Project(NetworkSessionContext context,
      object packet) {
    var dispatch = new OutboundDispatch(packet, PacketDispatchKind.Single, [context.Connection],
        allowedStages: NetworkSessionStage.AwaitPlayerData | NetworkSessionStage.Active);
    return ValueTask.FromResult(new PacketHandlingResult(true, [dispatch]));
  }
}
