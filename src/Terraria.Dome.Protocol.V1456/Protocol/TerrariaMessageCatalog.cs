using System;

namespace Terraria.Dome.Protocol.V1456.Protocol;

public enum TerrariaPacketDirection
{
  ClientToServer,
  ServerToClient,
  Bidirectional
}

public enum TerrariaPacketSupport
{
  Framed,
  Handled,
  Unsupported
}

public readonly record struct TerrariaMessageDescriptor(
  TerrariaMessageId MessageId,
  string Name,
  TerrariaPacketDirection Direction,
  TerrariaPacketSupport Support);

public static class TerrariaMessageCatalog
{
  private static readonly string[] Names = (
    "NeverCalled|Hello|Kick|PlayerInfo|SyncPlayer|SyncEquipment|RequestWorldData|WorldData|" +
    "SpawnTileData|StatusTextSize|TileSection|TileFrameSection|PlayerSpawn|PlayerControls|" +
    "PlayerActive|Unknown15|PlayerLifeMana|TileManipulation|SetTime|ToggleDoorState|" +
    "AreaTileChange|SyncItem|ItemOwner|SyncNPC|UnusedMeleeStrike|Unused25|Unused26|" +
    "SyncProjectile|DamageNPC|KillProjectile|TogglePVP|RequestChestOpen|SyncChestItem|" +
    "SyncPlayerChest|ChestUpdates|PlayerHeal|SyncPlayerZone|RequestPassword|SendPassword|" +
    "ReleaseItemOwnership|SyncTalkNPC|ItemRotationAndAnimation|Unknown42|ManaEffect|" +
    "Unknown44|TeamChange|OpenSignRequest|OpenSignResponse|LiquidUpdate|InitialSpawn|" +
    "PlayerBuffs|MiscDataSync|LockAndUnlock|AddNPCBuff|NPCBuffs|AddPlayerBuffPvP|" +
    "UniqueTownNPCInfoSyncRequest|Unknown57|InstrumentSound|HitSwitch|Unknown60|" +
    "SpawnBossUseLicenseStartEvent|Unknown62|SyncTilePaintOrCoating|SyncWallPaintOrCoating|" +
    "TeleportEntity|Unknown66|Unknown67|Unknown68|ChestName|BugCatching|BugReleasing|" +
    "TravelMerchantItems|RequestTeleportationByServer|AnglerQuest|AnglerQuestFinished|" +
    "QuestsCountSync|TemporaryAnimation|InvasionProgressReport|PlaceObject|" +
    "SyncPlayerChestIndex|CombatTextInt|NetModules|Unused83|PlayerStealth|QuickStackChests|" +
    "TileEntitySharing|TileEntityPlacement|ItemTweaker|ItemFrameTryPlacing|InstancedItem|" +
    "SyncEmoteBubble|SyncExtraValue|SocialHandshake|DevCommands|MurderSomeoneElsesPortal|" +
    "TeleportPlayerThroughPortal|AchievementMessageNPCKilled|AchievementMessageEventHappened|" +
    "MinionRestTargetUpdate|TeleportNPCThroughPortal|UpdateTowerShieldStrengths|" +
    "NebulaLevelupRequest|MoonlordHorror|ShopOverride|GemLockToggle|PoofOfSmoke|" +
    "SmartTextMessage|WiredCannonShot|MassWireOperation|MassWireOperationPay|ToggleParty|" +
    "SpecialFX|CrystalInvasionStart|CrystalInvasionWipeAllTheThingsss|" +
    "MinionAttackTargetUpdate|CrystalInvasionSendWaitTime|PlayerHurtV2|PlayerDeathV2|" +
    "CombatTextString|Emoji|TEDisplayDollDataSync|RequestTileEntityInteraction|" +
    "WeaponsRackTryPlacing|TEHatRackItemSync|SyncTilePicking|SyncRevengeMarker|" +
    "RemoveRevengeMarker|LandGolfBallInCup|FinishedConnectingToServer|FishOutNPC|" +
    "TamperWithNPC|PlayLegacySound|FoodPlatterTryPlacing|UpdatePlayerLuckFactors|" +
    "DeadPlayer|SyncCavernMonsterType|RequestNPCBuffRemoval|ClientSyncedInventory|" +
    "SetCountsAsHostForGameplay|SetMiscEventValues|RequestLucyPopup|SyncProjectileTrackers|" +
    "CrystalInvasionRequestedToSkipWaitTime|RequestQuestEffect|SyncItemsWithShimmer|" +
    "ShimmerActions|SyncLoadout|SyncItemCannotBeTakenByEnemies|" +
    "DeadCellsDisplayJarTryPlacing|SpectatePlayer|SyncItemDespawn|ItemUseSound|" +
    "NPCDebuffDamage|Ping|SyncChestSize|TELeashedEntityAnchorPlaceItem|" +
    "TeamChangeFromUI|ExtraSpawnSectionLoaded|RequestSection|ItemPosition|HostToken")
      .Split('|');

  private static readonly TerrariaMessageDescriptor[] Descriptors = CreateDescriptors();

  public static ReadOnlySpan<TerrariaMessageDescriptor> All => Descriptors;

  public static TerrariaMessageDescriptor Get(TerrariaMessageId messageId)
  {
    int index = (byte)messageId;
    if (index >= Descriptors.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(messageId));
    }

    return Descriptors[index];
  }

  private static TerrariaMessageDescriptor[] CreateDescriptors()
  {
    if (Names.Length != 162)
    {
      throw new InvalidOperationException("Terraria V1456 catalog does not cover IDs 0 through 161.");
    }

    TerrariaMessageDescriptor[] descriptors = new TerrariaMessageDescriptor[Names.Length];
    for (byte index = 0; index < descriptors.Length; index++)
    {
      descriptors[index] = new TerrariaMessageDescriptor(
        (TerrariaMessageId)index,
        Names[index],
        TerrariaPacketDirection.Bidirectional,
        TerrariaPacketSupport.Unsupported);
    }

    Set(descriptors, TerrariaMessageId.Hello, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SetUserSlot, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncPlayer, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncEquipment, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerLifeMana, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.ItemRotationAndAnimation,
      TerrariaPacketDirection.Bidirectional, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerBuffs, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.UniqueTownNpcInfoSyncRequest,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.NpcBuffs, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.WorldBiomeTypes, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.NpcHome, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerUuid, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.NetModules, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.AnglerQuest, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.TowerShieldStrengths,
      TerrariaPacketDirection.ServerToClient, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncLoadout, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.RequestWorldData, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SpawnTileData, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.StatusTextSize, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.TileSection, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.InitialSpawn, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerSpawn, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerControls, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncPlayerZone, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncTalkNpc, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.PlayerActive, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.TileManipulation,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SetTime,
      TerrariaPacketDirection.ServerToClient, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.ToggleDoorState,
      TerrariaPacketDirection.Bidirectional, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.OpenSignRequest,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.OpenSignResponse,
      TerrariaPacketDirection.Bidirectional, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.RequestChestOpen,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncChestItem,
      TerrariaPacketDirection.ServerToClient, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncPlayerChest,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncItem, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncNPC, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncProjectile, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.KillProjectile, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.FinishedConnectingToServer,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Framed);
    Set(descriptors, TerrariaMessageId.CavernMonsterTypes,
      TerrariaPacketDirection.ServerToClient, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.HostStatus, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.ClientSyncedInventory,
      TerrariaPacketDirection.ClientToServer, TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.Ping, TerrariaPacketDirection.Bidirectional,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.SyncChestSize, TerrariaPacketDirection.ServerToClient,
      TerrariaPacketSupport.Handled);
    Set(descriptors, TerrariaMessageId.RequestSection, TerrariaPacketDirection.ClientToServer,
      TerrariaPacketSupport.Handled);
    return descriptors;
  }

  private static void Set(
    TerrariaMessageDescriptor[] descriptors,
    TerrariaMessageId messageId,
    TerrariaPacketDirection direction,
    TerrariaPacketSupport support)
  {
    int index = (byte)messageId;
    TerrariaMessageDescriptor current = descriptors[index];
    descriptors[index] = current with { Direction = direction, Support = support };
  }
}
