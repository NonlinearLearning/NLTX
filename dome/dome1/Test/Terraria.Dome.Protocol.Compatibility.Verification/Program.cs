using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Isolation;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldGeneration;

LegacyWorldDataContext domeContext = LegacyWorldDataContext.CreateDomeDefaults();
NpcStatusEffectEnvelope npcStatusEffect = new(
  7,
  11,
  [new StatusEffectSnapshot(StatusEffectTargetKind.Npc, 7, 11, 119, 1800)]);
byte[] npcStatusEffectFrame = ContractExtensionCodec.EncodeNpcStatusEffect(npcStatusEffect);
NpcStatusEffectEnvelope decodedNpcStatusEffect =
  ContractExtensionCodec.DecodeNpcStatusEffect(npcStatusEffectFrame);
if (!ContractExtensionCodec.IsNpcStatusEffect(npcStatusEffectFrame) ||
    decodedNpcStatusEffect.ReplicationId != npcStatusEffect.ReplicationId ||
    decodedNpcStatusEffect.Revision != npcStatusEffect.Revision ||
    !decodedNpcStatusEffect.Effects.SequenceEqual(npcStatusEffect.Effects))
{
  throw new InvalidOperationException(
    "V1456 NPC status extension did not preserve bounded immutable status state.");
}

byte[] invasionProgressFrame = TerrariaPacketCodec.EncodeInvasionProgressReport(
  new InvasionProgressReportPacket(3, 120, -7, 2));
InvasionProgressReportPacket invasionProgress = TerrariaPacketCodec.DecodeInvasionProgressReport(
  invasionProgressFrame);
if (invasionProgress != new InvasionProgressReportPacket(3, 120, -7, 2) ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.InvasionProgressReport).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 InvasionProgressReport did not preserve its source-shaped payload.");
}

byte[] chestIndexFrame = TerrariaPacketCodec.EncodeSyncPlayerChestIndex(
  new SyncPlayerChestIndexPacket(7, -12));
SyncPlayerChestIndexPacket chestIndex = TerrariaPacketCodec.DecodeSyncPlayerChestIndex(
  chestIndexFrame);
if (chestIndex != new SyncPlayerChestIndexPacket(7, -12) ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncPlayerChestIndex).Direction !=
      TerrariaPacketDirection.ServerToClient)
{
  throw new InvalidOperationException(
    "V1456 SyncPlayerChestIndex did not preserve its source-shaped payload.");
}

ChestNamePacket chestName = new(-1, 120, 240, "宝箱-A");
ChestNamePacket decodedChestName = TerrariaPacketCodec.DecodeChestName(
  TerrariaPacketCodec.EncodeChestName(chestName));
if (decodedChestName != chestName ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.ChestName).Direction !=
      TerrariaPacketDirection.ServerToClient)
{
  throw new InvalidOperationException("V1456 ChestName did not preserve its source-shaped string payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeChestName(
    TerrariaPacketCodec.EncodeChestName(chestName)[..^1]);
  throw new InvalidOperationException("V1456 ChestName accepted a truncated string payload.");
}
catch (InvalidDataException)
{
}

PlayerHurtV2Packet playerHurt = new(3, new byte[] { 0x20, 0xFE, 0x01 }, -42, 1, 3, -2);
PlayerHurtV2Packet decodedHurt = TerrariaPacketCodec.DecodePlayerHurtV2(
  TerrariaPacketCodec.Encode(playerHurt));
if (
    decodedHurt.PlayerId != playerHurt.PlayerId ||
    !decodedHurt.DeathReasonPayload.AsSpan().SequenceEqual(playerHurt.DeathReasonPayload) ||
    decodedHurt.Damage != playerHurt.Damage || decodedHurt.Direction != playerHurt.Direction ||
    decodedHurt.Flags != playerHurt.Flags || decodedHurt.CooldownCounter != playerHurt.CooldownCounter ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerHurtV2).Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 PlayerHurtV2 did not preserve its variable reason payload and fixed trailer.");
}

PlayerDeathV2Packet playerDeath = new(4, new byte[] { 0x40, 0x02 }, 77, 0, 1);
PlayerDeathV2Packet decodedDeath = TerrariaPacketCodec.DecodePlayerDeathV2(
  TerrariaPacketCodec.Encode(playerDeath));
if (decodedDeath.PlayerId != playerDeath.PlayerId ||
    !decodedDeath.DeathReasonPayload.AsSpan().SequenceEqual(playerDeath.DeathReasonPayload) ||
    decodedDeath.Damage != playerDeath.Damage || decodedDeath.Direction != playerDeath.Direction ||
    decodedDeath.Pvp != playerDeath.Pvp ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerDeathV2).Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 PlayerDeathV2 did not preserve its variable reason payload and fixed trailer.");
}

CombatTextStringPacket combatTextString = new(1.5f, -2.5f, new TerrariaColor(4, 5, 6),
  new byte[] { 0, 3, 0x41, 0x42 });
CombatTextStringPacket decodedCombatTextString = TerrariaPacketCodec.DecodeCombatTextString(
  TerrariaPacketCodec.Encode(combatTextString));
if (decodedCombatTextString.PositionX != combatTextString.PositionX ||
    decodedCombatTextString.PositionY != combatTextString.PositionY ||
    decodedCombatTextString.Color != combatTextString.Color ||
    !decodedCombatTextString.TextPayload.AsSpan().SequenceEqual(combatTextString.TextPayload) ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CombatTextString).Direction !=
      TerrariaPacketDirection.ServerToClient)
{
  throw new InvalidOperationException(
    "V1456 CombatTextString did not preserve its source-shaped text payload.");
}

EmojiPacket emoji = new(6, 42);
if (TerrariaPacketCodec.DecodeEmoji(TerrariaPacketCodec.Encode(emoji)) != emoji ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.Emoji).Direction != TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.Emoji).Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 Emoji did not preserve its two-byte payload.");
}

DisplayDollDataSyncPacket doll = new(2, 901, 4, 7, new byte[] { 1, 2, 3, 4 });
DisplayDollDataSyncPacket decodedDoll = TerrariaPacketCodec.DecodeDisplayDollDataSync(
  TerrariaPacketCodec.Encode(doll));
if (decodedDoll.PlayerId != doll.PlayerId || decodedDoll.EntityId != doll.EntityId ||
    decodedDoll.Slot != doll.Slot || decodedDoll.Param != doll.Param ||
    !decodedDoll.DataPayload.AsSpan().SequenceEqual(doll.DataPayload) ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TedisplayDollDataSync).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 TEDisplayDollDataSync did not preserve its variable data payload.");
}

RequestTileEntityInteractionPacket tileEntityRequest = new(-1, 9);
if (TerrariaPacketCodec.DecodeRequestTileEntityInteraction(
      TerrariaPacketCodec.Encode(tileEntityRequest)) != tileEntityRequest ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.RequestTileEntityInteraction).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.RequestTileEntityInteraction).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 RequestTileEntityInteraction did not preserve its five-byte payload.");
}

WeaponsRackTryPlacingPacket rack = new(-12, 34, 100, 5, 2);
if (TerrariaPacketCodec.DecodeWeaponsRackTryPlacing(TerrariaPacketCodec.Encode(rack)) != rack ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.WeaponsRackTryPlacing).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.WeaponsRackTryPlacing).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 WeaponsRackTryPlacing did not preserve its nine-byte payload.");
}

HatRackItemSyncPacket hatRack = new(5, 700, 1, true, 88, 3);
if (TerrariaPacketCodec.DecodeHatRackItemSync(TerrariaPacketCodec.Encode(hatRack)) != hatRack ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TehatRackItemSync).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TehatRackItemSync).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 TEHatRackItemSync did not preserve its eleven-byte payload.");
}

SyncPlayerChestLocationPacket chestLocation = new(3, -10, 25, 2);
if (TerrariaPacketCodec.DecodeSyncPlayerChestLocation(
      TerrariaPacketCodec.Encode(chestLocation)) != chestLocation ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncPlayerChestLocation).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncPlayerChestLocation).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 SyncPlayerChestLocation did not preserve its six-byte payload.");
}

SyncRevengeMarkerPacket revengeMarker = new(-123);
if (TerrariaPacketCodec.DecodeSyncRevengeMarker(
      TerrariaPacketCodec.Encode(revengeMarker)) != revengeMarker ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncRevengeMarker).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncRevengeMarker).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 SyncRevengeMarker did not preserve its two-byte payload.");
}

RemoveRevengeMarkerPacket removeMarker = new(-987654);
if (TerrariaPacketCodec.DecodeRemoveRevengeMarker(
      TerrariaPacketCodec.Encode(removeMarker)) != removeMarker ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.RemoveRevengeMarker).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.RemoveRevengeMarker).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 RemoveRevengeMarker did not preserve its four-byte payload.");
}

LandGolfBallInCupPacket golf = new(7, 100, 200, 300, 400);
if (TerrariaPacketCodec.DecodeLandGolfBallInCup(TerrariaPacketCodec.Encode(golf)) != golf ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.LandGolfBallInCup).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.LandGolfBallInCup).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 LandGolfBallInCup did not preserve its nine-byte payload.");
}

FishOutNpcPacket fishOut = new(160, 320, -682);
if (TerrariaPacketCodec.DecodeFishOutNpc(TerrariaPacketCodec.Encode(fishOut)) != fishOut ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.FishOutNpc).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.FishOutNpc).Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 FishOutNPC did not preserve its six-byte payload.");
}

CrystalInvasionNextWaveWaitPacket invasionWait = new(321);
if (TerrariaPacketCodec.DecodeCrystalInvasionNextWaveWait(
      TerrariaPacketCodec.Encode(invasionWait)) != invasionWait ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionSendWaitTime).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionSendWaitTime).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 CrystalInvasionSendWaitTime did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeCrystalInvasionNextWaveWait(
    TerrariaPacketCodec.Encode(invasionWait)[..^1]);
  throw new InvalidOperationException(
    "V1456 CrystalInvasionSendWaitTime accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

short[] merchantItems = new short[TravelMerchantItemsPacket.SlotCount];
merchantItems[0] = -1;
merchantItems[^1] = 321;
TravelMerchantItemsPacket merchantPacket = new(merchantItems);
TravelMerchantItemsPacket decodedMerchantPacket = TerrariaPacketCodec.DecodeTravelMerchantItems(
  TerrariaPacketCodec.EncodeTravelMerchantItems(merchantPacket));
if (!decodedMerchantPacket.ItemIds.SequenceEqual(merchantItems) ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TravelMerchantItems).Support !=
      TerrariaPacketSupport.Unsupported)
{
  throw new InvalidOperationException("V1456 TravelMerchantItems did not preserve its fixed table boundary.");
}

try
{
  _ = TerrariaPacketCodec.DecodeTravelMerchantItems(
    TerrariaPacketCodec.EncodeTravelMerchantItems(merchantPacket)[..^1]);
  throw new InvalidOperationException("V1456 TravelMerchantItems accepted a truncated table.");
}
catch (InvalidDataException)
{
}

BugCatchingPacket bugCatch = new(-1, 255);
if (TerrariaPacketCodec.DecodeBugCatching(TerrariaPacketCodec.EncodeBugCatching(bugCatch)) != bugCatch ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.BugCatching).Support != TerrariaPacketSupport.Framed)
{
  throw new InvalidOperationException("V1456 BugCatching did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeBugCatching(
    TerrariaPacketCodec.EncodeBugCatching(bugCatch)[..^1]);
  throw new InvalidOperationException("V1456 BugCatching accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

BugReleasingPacket bugRelease = new(-100, 200, -7, 255);
if (TerrariaPacketCodec.DecodeBugReleasing(TerrariaPacketCodec.EncodeBugReleasing(bugRelease)) !=
    bugRelease ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.BugReleasing).Support != TerrariaPacketSupport.Framed)
{
  throw new InvalidOperationException("V1456 BugReleasing did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeBugReleasing(
    TerrariaPacketCodec.EncodeBugReleasing(bugRelease)[..^1]);
  throw new InvalidOperationException("V1456 BugReleasing accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

CombatTextIntPacket combatText = new(
  12.5f,
  -4.25f,
  new TerrariaColor(1, 2, 3),
  -900);
if (TerrariaPacketCodec.DecodeCombatTextInt(TerrariaPacketCodec.EncodeCombatTextInt(combatText)) !=
    combatText ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CombatTextInt).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 CombatTextInt did not preserve its source-shaped payload.");
}

TeleportEntityPacket teleportEntity = new(
  Flags: 0x08,
  TargetId: 17,
  PositionX: 123.5f,
  PositionY: -42.25f,
  Style: 2,
  ExtraInfo: 901);
if (TerrariaPacketCodec.DecodeTeleportEntity(TerrariaPacketCodec.Encode(teleportEntity)) !=
    teleportEntity ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TeleportEntity).Direction !=
      TerrariaPacketDirection.ServerToClient)
{
  throw new InvalidOperationException(
    "V1456 TeleportEntity did not preserve its optional source-shaped payload.");
}

TeleportEntityPacket teleportWithoutExtra = new(0, 4, 1.0f, 2.0f, 0, null);
if (TerrariaPacketCodec.DecodeTeleportEntity(TerrariaPacketCodec.Encode(teleportWithoutExtra)).ExtraInfo is not null)
{
  throw new InvalidOperationException("V1456 TeleportEntity incorrectly decoded absent extra info.");
}

try
{
  _ = TerrariaPacketCodec.DecodeTeleportEntity(
    TerrariaPacketCodec.Encode(teleportEntity)[..^1]);
  throw new InvalidOperationException("V1456 TeleportEntity accepted a truncated optional field.");
}
catch (InvalidDataException)
{
}

PlayerHealOtherPacket playerHealOther = new(12, -25);
if (TerrariaPacketCodec.DecodePlayerHealOther(
      TerrariaPacketCodec.Encode(playerHealOther)) != playerHealOther ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerHealOther).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerHealOther).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 PlayerHealOther did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodePlayerHealOther(
    TerrariaPacketCodec.Encode(playerHealOther)[..^1]);
  throw new InvalidOperationException("V1456 PlayerHealOther accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

TerrariaMessageDescriptor unused67 = TerrariaMessageCatalog.Get(TerrariaMessageId.Unused67);
if (unused67.Name != "Unused67" || unused67.Support != TerrariaPacketSupport.Unsupported)
{
  throw new InvalidOperationException(
    "V1456 message 67 must remain an explicitly isolated legacy no-op.");
}

PlayerStealthPacket playerStealth = new(9, 0.375f);
if (TerrariaPacketCodec.DecodePlayerStealth(TerrariaPacketCodec.Encode(playerStealth)) !=
    playerStealth ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerStealth).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PlayerStealth).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 PlayerStealth did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodePlayerStealth(
    TerrariaPacketCodec.Encode(playerStealth)[..^1]);
  throw new InvalidOperationException("V1456 PlayerStealth accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

SyncExtraValuePacket syncExtraValue = new(-4, 123456, 12.5f, -8.25f);
if (TerrariaPacketCodec.DecodeSyncExtraValue(TerrariaPacketCodec.Encode(syncExtraValue)) !=
    syncExtraValue ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncExtraValue).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SyncExtraValue).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 SyncExtraValue did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeSyncExtraValue(
    TerrariaPacketCodec.Encode(syncExtraValue)[..^1]);
  throw new InvalidOperationException("V1456 SyncExtraValue accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

TeleportPlayerThroughPortalPacket portalTeleport = new(
  5,
  -13,
  100.25f,
  -50.5f,
  2.0f,
  -3.75f);
if (TerrariaPacketCodec.DecodeTeleportPlayerThroughPortal(
      TerrariaPacketCodec.Encode(portalTeleport)) != portalTeleport ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TeleportPlayerThroughPortal).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TeleportPlayerThroughPortal).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 TeleportPlayerThroughPortal did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeTeleportPlayerThroughPortal(
    TerrariaPacketCodec.Encode(portalTeleport)[..^1]);
  throw new InvalidOperationException(
    "V1456 TeleportPlayerThroughPortal accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

MinionRestTargetUpdatePacket minionRest = new(3, 45.5f, -12.25f);
if (TerrariaPacketCodec.DecodeMinionRestTargetUpdate(
      TerrariaPacketCodec.Encode(minionRest)) != minionRest ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MinionRestTargetUpdate).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MinionRestTargetUpdate).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 MinionRestTargetUpdate did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeMinionRestTargetUpdate(
    TerrariaPacketCodec.Encode(minionRest)[..^1]);
  throw new InvalidOperationException(
    "V1456 MinionRestTargetUpdate accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

TeleportNpcThroughPortalPacket npcPortal = new(
  60000,
  -9,
  10.5f,
  -20.25f,
  1.75f,
  -2.5f);
if (TerrariaPacketCodec.DecodeTeleportNpcThroughPortal(
      TerrariaPacketCodec.Encode(npcPortal)) != npcPortal ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TeleportNpcThroughPortal).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TeleportNpcThroughPortal).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 TeleportNpcThroughPortal did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeTeleportNpcThroughPortal(
    TerrariaPacketCodec.Encode(npcPortal)[..^1]);
  throw new InvalidOperationException(
    "V1456 TeleportNpcThroughPortal accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

NebulaLevelupRequestPacket nebula = new(7, 179, 320.5f, -44.75f);
if (TerrariaPacketCodec.DecodeNebulaLevelupRequest(TerrariaPacketCodec.Encode(nebula)) != nebula ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.NebulaLevelupRequest).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.NebulaLevelupRequest).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 NebulaLevelupRequest did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeNebulaLevelupRequest(
    TerrariaPacketCodec.Encode(nebula)[..^1]);
  throw new InvalidOperationException("V1456 NebulaLevelupRequest accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

MoonlordHorrorPacket moonlordHorror = new(1200, -35);
if (TerrariaPacketCodec.DecodeMoonlordHorror(TerrariaPacketCodec.Encode(moonlordHorror)) !=
    moonlordHorror ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MoonlordHorror).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MoonlordHorror).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 MoonlordHorror did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeMoonlordHorror(
    TerrariaPacketCodec.Encode(moonlordHorror)[..^1]);
  throw new InvalidOperationException("V1456 MoonlordHorror accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

GemLockTogglePacket gemLock = new(-120, 340, true);
if (TerrariaPacketCodec.DecodeGemLockToggle(TerrariaPacketCodec.Encode(gemLock)) != gemLock ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.GemLockToggle).Direction !=
      TerrariaPacketDirection.Bidirectional ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.GemLockToggle).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 GemLockToggle did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeGemLockToggle(
    TerrariaPacketCodec.Encode(gemLock)[..^1]);
  throw new InvalidOperationException("V1456 GemLockToggle accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

PoofOfSmokePacket poof = new(0xC1234567u);
if (TerrariaPacketCodec.DecodePoofOfSmoke(TerrariaPacketCodec.Encode(poof)) != poof ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PoofOfSmoke).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.PoofOfSmoke).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 PoofOfSmoke did not preserve its packed source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodePoofOfSmoke(
    TerrariaPacketCodec.Encode(poof)[..^1]);
  throw new InvalidOperationException("V1456 PoofOfSmoke accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

ShopOverridePacket shopOverride = new(4, 521, 3, 7, -1200, 0xA5);
if (TerrariaPacketCodec.DecodeShopOverride(TerrariaPacketCodec.Encode(shopOverride)) !=
    shopOverride ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.ShopOverride).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.ShopOverride).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 ShopOverride did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeShopOverride(TerrariaPacketCodec.Encode(shopOverride)[..^1]);
  throw new InvalidOperationException("V1456 ShopOverride accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

WiredCannonShotPacket cannonShot = new(45, 3.5f, -10, 20, 90, 521, 6);
if (TerrariaPacketCodec.DecodeWiredCannonShot(TerrariaPacketCodec.Encode(cannonShot)) !=
    cannonShot ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.WiredCannonShot).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.WiredCannonShot).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 WiredCannonShot did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeWiredCannonShot(
    TerrariaPacketCodec.Encode(cannonShot)[..^1]);
  throw new InvalidOperationException("V1456 WiredCannonShot accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

MassWireOperationPacket wireOperation = new(-10, 20, 30, -40, 3);
if (TerrariaPacketCodec.DecodeMassWireOperation(TerrariaPacketCodec.Encode(wireOperation)) !=
    wireOperation ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MassWireOperation).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MassWireOperation).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 MassWireOperation did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeMassWireOperation(
    TerrariaPacketCodec.Encode(wireOperation)[..^1]);
  throw new InvalidOperationException("V1456 MassWireOperation accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

MassWireOperationPayPacket wirePay = new(-25, 4, 9);
if (TerrariaPacketCodec.DecodeMassWireOperationPay(TerrariaPacketCodec.Encode(wirePay)) !=
    wirePay ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MassWireOperationPay).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MassWireOperationPay).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 MassWireOperationPay did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeMassWireOperationPay(
    TerrariaPacketCodec.Encode(wirePay)[..^1]);
  throw new InvalidOperationException(
    "V1456 MassWireOperationPay accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

SpecialFxPacket specialFx = new(2, 1000, -2000, 4, -12, 7);
if (TerrariaPacketCodec.DecodeSpecialFx(TerrariaPacketCodec.Encode(specialFx)) != specialFx ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SpecialFx).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.SpecialFx).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 SpecialFX did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeSpecialFx(TerrariaPacketCodec.Encode(specialFx)[..^1]);
  throw new InvalidOperationException("V1456 SpecialFX accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

CrystalInvasionStartPacket crystalStart = new(-300, 450);
if (TerrariaPacketCodec.DecodeCrystalInvasionStart(TerrariaPacketCodec.Encode(crystalStart)) !=
    crystalStart ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionStart).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionStart).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 CrystalInvasionStart did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeCrystalInvasionStart(
    TerrariaPacketCodec.Encode(crystalStart)[..^1]);
  throw new InvalidOperationException("V1456 CrystalInvasionStart accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

CrystalInvasionWipePacket crystalWipe = new();
if (TerrariaPacketCodec.DecodeCrystalInvasionWipe(
      TerrariaPacketCodec.Encode(crystalWipe)) != crystalWipe ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionWipeAllTheThingsss).Direction !=
      TerrariaPacketDirection.ServerToClient ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.CrystalInvasionWipeAllTheThingsss).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 CrystalInvasionWipe did not preserve its empty source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeCrystalInvasionWipe(
    TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.CrystalInvasionWipeAllTheThingsss,
      new byte[] { 1 })));
  throw new InvalidOperationException("V1456 CrystalInvasionWipe accepted a non-empty payload.");
}
catch (InvalidDataException)
{
}

MinionAttackTargetUpdatePacket minionAttack = new(8, -17);
if (TerrariaPacketCodec.DecodeMinionAttackTargetUpdate(
      TerrariaPacketCodec.Encode(minionAttack)) != minionAttack ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MinionAttackTargetUpdate).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.MinionAttackTargetUpdate).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "V1456 MinionAttackTargetUpdate did not preserve its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeMinionAttackTargetUpdate(
    TerrariaPacketCodec.Encode(minionAttack)[..^1]);
  throw new InvalidOperationException(
    "V1456 MinionAttackTargetUpdate accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

TemporaryAnimationPacket animation = new(-2, 600, -12, 34);
if (TerrariaPacketCodec.DecodeTemporaryAnimation(TerrariaPacketCodec.EncodeTemporaryAnimation(animation)) !=
    animation ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.TemporaryAnimation).Direction !=
      TerrariaPacketDirection.ServerToClient)
{
  throw new InvalidOperationException("V1456 TemporaryAnimation did not preserve its source-shaped payload.");
}

QuestsCountSyncPacket questCounts = new(9, -3, 42);
if (TerrariaPacketCodec.DecodeQuestsCountSync(TerrariaPacketCodec.EncodeQuestsCountSync(questCounts)) !=
    questCounts ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.QuestsCountSync).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 QuestsCountSync did not preserve its source-shaped payload.");
}

byte[] anglerFinishedFrame = TerrariaPacketCodec.EncodeAnglerQuestFinished(
  new AnglerQuestFinishedPacket());
_ = TerrariaPacketCodec.DecodeAnglerQuestFinished(anglerFinishedFrame);
if (TerrariaMessageCatalog.Get(TerrariaMessageId.AnglerQuestFinished).Direction !=
      TerrariaPacketDirection.ClientToServer ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.AnglerQuestFinished).Support !=
      TerrariaPacketSupport.Framed)
{
  throw new InvalidOperationException("V1456 AnglerQuestFinished did not preserve its framed boundary.");
}

RequestTeleportationByServerPacket teleportRequest = new(4);
if (TerrariaPacketCodec.DecodeRequestTeleportationByServer(
      TerrariaPacketCodec.EncodeRequestTeleportationByServer(teleportRequest)) != teleportRequest ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.RequestTeleportationByServer).Support !=
      TerrariaPacketSupport.Framed)
{
  throw new InvalidOperationException(
    "V1456 RequestTeleportationByServer did not preserve its selector boundary.");
}

try
{
  _ = TerrariaPacketCodec.EncodeRequestTeleportationByServer(new RequestTeleportationByServerPacket(5));
  throw new InvalidOperationException("V1456 RequestTeleportationByServer accepted an invalid selector.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeAnglerQuestFinished(
    TerrariaFrameCodec.Encode(new TerrariaFrame(
      TerrariaMessageId.AnglerQuestFinished,
      new byte[] { 1 })));
  throw new InvalidOperationException("V1456 AnglerQuestFinished accepted a non-empty payload.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeQuestsCountSync(
    TerrariaPacketCodec.EncodeQuestsCountSync(questCounts)[..^1]);
  throw new InvalidOperationException("V1456 QuestsCountSync accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeTemporaryAnimation(
    TerrariaPacketCodec.EncodeTemporaryAnimation(animation)[..^1]);
  throw new InvalidOperationException("V1456 TemporaryAnimation accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCombatTextInt(
    TerrariaPacketCodec.EncodeCombatTextInt(combatText)[..^1]);
  throw new InvalidOperationException("V1456 CombatTextInt accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeSyncPlayerChestIndex(chestIndexFrame[..^1]);
  throw new InvalidOperationException("V1456 SyncPlayerChestIndex accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeInvasionProgressReport(invasionProgressFrame[..^1]);
  throw new InvalidOperationException("V1456 InvasionProgressReport accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

byte[] tracedHello =
[
  0x0F, 0x00, (byte)TerrariaMessageId.Hello, 0x0B,
  (byte)'T', (byte)'e', (byte)'r', (byte)'r', (byte)'a', (byte)'r', (byte)'i',
  (byte)'a', (byte)'3', (byte)'1', (byte)'9'
];
HelloPacket tracedHelloPacket = TerrariaPacketCodec.DecodeHello(tracedHello);
if (tracedHelloPacket.ProtocolIdentifier != TerrariaProtocolVersion.HelloIdentifier)
{
  throw new InvalidOperationException("V1456 Hello raw frame did not retain its protocol string.");
}

SetUserSlotPacket tracedUserSlot = TerrariaPacketCodec.DecodeSetUserSlot(
  Convert.FromHexString("0500030100"));
if (tracedUserSlot.PlayerSlot != 1 || tracedUserSlot.IsServerSideCharacter)
{
  throw new InvalidOperationException("V1456 SetUserSlot raw frame did not retain its fields.");
}

ClientTalkNpcPacket tracedTalkNpc = TerrariaPacketCodec.DecodeClientTalkNpc(
  Convert.FromHexString("06002801FFFF"));
if (tracedTalkNpc.PlayerSlot != 1 || tracedTalkNpc.TalkNpc != -1)
{
  throw new InvalidOperationException("V1456 SyncTalkNpc raw frame did not retain its fields.");
}

TerrariaPacketCodec.ValidateClientSyncedInventory(Convert.FromHexString("03008A"));

NetModulePacket tracedOpaqueNetModule = TerrariaPacketCodec.DecodeNetModule(
  Convert.FromHexString("0800520C00DEADBE"));
if (tracedOpaqueNetModule.ModuleId != 12 ||
    !tracedOpaqueNetModule.Payload.Span.SequenceEqual(new byte[] { 0xDE, 0xAD, 0xBE }))
{
  throw new InvalidOperationException(
    "V1456 NetModules raw frame did not retain its module boundary and payload.");
}

BannerModulePacket defaultBanner = TerrariaPacketCodec.DecodeBannerModule(
  TerrariaPacketCodec.CreateDefaultJoinStateNetModules()[0]);
if (defaultBanner.MessageType != BannerModuleMessageType.FullState ||
    defaultBanner.KillCounts is null || defaultBanner.ClaimableCounts is null ||
    defaultBanner.KillCounts.Length != 293 || defaultBanner.ClaimableCounts.Length != 293 ||
    !Array.TrueForAll(defaultBanner.KillCounts, value => value == 0) ||
    !Array.TrueForAll(defaultBanner.ClaimableCounts, value => value == 0))
{
  throw new InvalidOperationException(
    "V1456 Banners default join-state did not retain its source-shaped full state.");
}

BannerModulePacket tracedBannerFullState = TerrariaPacketCodec.DecodeBannerModule(
  CreateBannerModuleFrame(BannerModuleMessageType.FullState, writer =>
  {
    writer.Write((short)2);
    writer.Write(100);
    writer.Write(-200);
    writer.Write((short)3);
    writer.Write((ushort)4);
    writer.Write((ushort)5);
    writer.Write((ushort)6);
  }));
if (tracedBannerFullState.KillCounts is null ||
    !tracedBannerFullState.KillCounts.SequenceEqual(new int[] { 100, -200 }) ||
    tracedBannerFullState.ClaimableCounts is null ||
    !tracedBannerFullState.ClaimableCounts.SequenceEqual(new ushort[] { 4, 5, 6 }))
{
  throw new InvalidOperationException(
    "V1456 Banners FullState did not retain its variable source arrays.");
}

BannerModulePacket tracedBannerKillUpdate = TerrariaPacketCodec.DecodeBannerModule(
  CreateBannerModuleFrame(BannerModuleMessageType.KillCountUpdate, writer =>
  {
    writer.Write((short)17);
    writer.Write(123456);
  }));
if (tracedBannerKillUpdate.BannerId != 17 || tracedBannerKillUpdate.KillCount != 123456)
{
  throw new InvalidOperationException(
    "V1456 Banners KillCountUpdate did not retain its fixed payload.");
}

BannerModulePacket tracedBannerClaimUpdate = TerrariaPacketCodec.DecodeBannerModule(
  CreateBannerModuleFrame(BannerModuleMessageType.ClaimCountUpdate, writer =>
  {
    writer.Write((short)18);
    writer.Write((ushort)321);
  }));
if (tracedBannerClaimUpdate.BannerId != 18 || tracedBannerClaimUpdate.ClaimCount != 321)
{
  throw new InvalidOperationException(
    "V1456 Banners ClaimCountUpdate did not retain its fixed payload.");
}

BannerModulePacket tracedBannerRequest = TerrariaPacketCodec.DecodeBannerModule(
  CreateBannerModuleFrame(BannerModuleMessageType.ClaimRequest, writer =>
  {
    writer.Write((short)19);
    writer.Write((ushort)12);
  }));
if (tracedBannerRequest.BannerId != 19 || tracedBannerRequest.ClaimCount != 12)
{
  throw new InvalidOperationException(
    "V1456 Banners ClaimRequest did not retain its fixed payload.");
}

BannerModulePacket tracedBannerResponse = TerrariaPacketCodec.DecodeBannerModule(
  CreateBannerModuleFrame(BannerModuleMessageType.ClaimResponse, writer =>
  {
    writer.Write((short)20);
    writer.Write((ushort)11);
    writer.Write(true);
  }));
if (tracedBannerResponse.BannerId != 20 || tracedBannerResponse.ClaimCount != 11 ||
    tracedBannerResponse.Granted != true)
{
  throw new InvalidOperationException(
    "V1456 Banners ClaimResponse did not retain its fixed payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeBannerModule(
    CreateBannerModuleFrame((BannerModuleMessageType)5));
  throw new InvalidOperationException("V1456 Banners accepted an unknown message type.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeBannerModule(
    CreateBannerModuleFrame(BannerModuleMessageType.FullState, writer =>
    {
      writer.Write((short)1);
      writer.Write(42);
    }));
  throw new InvalidOperationException("V1456 Banners accepted a truncated FullState payload.");
}
catch (InvalidDataException)
{
}

TerrariaSession activeBannerSession = CreateActiveSession(7);
NetModulePacket activeBanner = activeBannerSession.AcceptNetModule(
  CreateBannerModuleFrame(BannerModuleMessageType.ClaimRequest, writer =>
  {
    writer.Write((short)19);
    writer.Write((ushort)12);
  }));
if (activeBanner.ModuleId != 10 || activeBannerSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 Banners ClaimRequest did not preserve active-session compatibility handling.");
}

CraftingRequestModulePacket tracedCraftingRequest = TerrariaPacketCodec.DecodeCraftingRequestModule(
  CreateCraftingModuleFrame(writer =>
  {
    writer.Write7BitEncodedInt(2);
    writer.Write(100);
    writer.Write7BitEncodedInt(300);
    writer.Write(-5);
    writer.Write7BitEncodedInt(-1);
    writer.Write7BitEncodedInt(2);
    writer.Write7BitEncodedInt(-1);
    writer.Write7BitEncodedInt(42);
  }));
if (tracedCraftingRequest.Items.Count != 2 ||
    tracedCraftingRequest.Items[0] != new CraftingRequiredItemPacket(100, 300) ||
    tracedCraftingRequest.Items[1] != new CraftingRequiredItemPacket(-5, -1) ||
    !tracedCraftingRequest.ChestIndices.SequenceEqual(new int[] { -1, 42 }))
{
  throw new InvalidOperationException(
    "V1456 CraftingRequests did not retain its 7-bit lists and signed item fields.");
}

CraftingResponseModulePacket tracedCraftingResponse =
  TerrariaPacketCodec.DecodeCraftingResponseModule(
    CreateCraftingModuleFrame(writer => writer.Write(true)));
if (!tracedCraftingResponse.Approved)
{
  throw new InvalidOperationException("V1456 CraftingRequests response did not retain approval.");
}

TerrariaSession activeCraftingSession = CreateActiveSession(7);
NetModulePacket activeCrafting = activeCraftingSession.AcceptNetModule(
  CreateCraftingModuleFrame(writer =>
  {
    writer.Write7BitEncodedInt(0);
    writer.Write7BitEncodedInt(0);
  }));
if (activeCrafting.ModuleId != 11 || activeCraftingSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 CraftingRequests did not preserve active-session compatibility handling.");
}

try
{
  _ = TerrariaPacketCodec.DecodeCraftingRequestModule(
    CreateCraftingModuleFrame(writer =>
    {
      writer.Write7BitEncodedInt(0);
      writer.Write7BitEncodedInt(0);
      writer.Write((byte)0);
    }));
  throw new InvalidOperationException("V1456 CraftingRequests accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCraftingRequestModule(
    CreateCraftingModuleFrame(writer => writer.Write7BitEncodedInt(-1)));
  throw new InvalidOperationException("V1456 CraftingRequests accepted a negative item count.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCraftingRequestModule(
    CreateCraftingModuleFrame(writer =>
    {
      writer.Write7BitEncodedInt(1);
    }));
  throw new InvalidOperationException("V1456 CraftingRequests accepted a truncated item list.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCraftingRequestModule(
    CreateCraftingModuleFrame(writer =>
    {
      writer.Write(new byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 });
    }));
  throw new InvalidOperationException("V1456 CraftingRequests accepted an invalid 7-bit count.");
}
catch (InvalidDataException)
{
}

TagEffectModulePacket tracedTagFullState = TerrariaPacketCodec.DecodeTagEffectModule(
  CreateTagEffectModuleFrame(7, TagEffectMessageType.FullState, writer =>
  {
    writer.Write((short)5478);
    writer.Write((byte)3);
    writer.Write(100);
    writer.Write((byte)199);
    writer.Write(-5);
    writer.Write((byte)200);
  }));
if (tracedTagFullState.OwnerSlot != 7 ||
    tracedTagFullState.EffectType != 5478 ||
    tracedTagFullState.TaggedNpcTimes is null ||
    !tracedTagFullState.TaggedNpcTimes.SequenceEqual(new TagEffectSparseEntry[]
    {
      new(3, 100),
      new(199, -5)
    }))
{
  throw new InvalidOperationException(
    "V1456 TagEffect FullState did not retain sparse NPC time entries.");
}

TagEffectModulePacket tracedTagChange = TerrariaPacketCodec.DecodeTagEffectModule(
  CreateTagEffectModuleFrame(8, TagEffectMessageType.ChangeActiveEffect, writer =>
  {
    writer.Write((short)4914);
  }));
if (tracedTagChange.OwnerSlot != 8 || tracedTagChange.EffectType != 4914)
{
  throw new InvalidOperationException(
    "V1456 TagEffect ChangeActiveEffect did not retain its source fields.");
}

foreach (TagEffectMessageType messageType in new TagEffectMessageType[]
{
  TagEffectMessageType.ApplyTagToNpc,
  TagEffectMessageType.EnableProcOnNpc,
  TagEffectMessageType.ClearProcOnNpc
})
{
  TagEffectModulePacket packet = TerrariaPacketCodec.DecodeTagEffectModule(
    CreateTagEffectModuleFrame(9, messageType, writer => writer.Write((byte)199)));
  if (packet.MessageType != messageType || packet.NpcIndex != 199)
  {
    throw new InvalidOperationException(
      $"V1456 TagEffect {messageType} did not retain its NPC index.");
  }
}

try
{
  _ = TerrariaPacketCodec.DecodeTagEffectModule(
    CreateTagEffectModuleFrame(7, (TagEffectMessageType)5));
  throw new InvalidOperationException("V1456 TagEffect accepted an unknown message type.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeTagEffectModule(
    CreateTagEffectModuleFrame(7, TagEffectMessageType.FullState, writer =>
    {
      writer.Write((short)5478);
      writer.Write((byte)3);
      writer.Write(100);
    }));
  throw new InvalidOperationException("V1456 TagEffect accepted an unterminated sparse array.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeTagEffectModule(
    CreateTagEffectModuleFrame(7, TagEffectMessageType.ChangeActiveEffect, writer =>
    {
      writer.Write((short)1);
      writer.Write((byte)0);
    }));
  throw new InvalidOperationException("V1456 TagEffect accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

TerrariaSession activeTagSession = CreateActiveSession(7);
NetModulePacket activeTag = activeTagSession.AcceptNetModule(
  CreateTagEffectModuleFrame(7, TagEffectMessageType.ClearProcOnNpc, writer =>
  {
    writer.Write((byte)12);
  }));
if (activeTag.ModuleId != 12 || activeTagSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 TagEffect did not preserve active-session compatibility handling.");
}

LeashedEntityModulePacket tracedLeashedRemove = TerrariaPacketCodec.DecodeLeashedEntityModule(
  CreateLeashedEntityModuleFrame(LeashedEntityMessageType.Remove, 300));
if (tracedLeashedRemove.Slot != 300 || tracedLeashedRemove.EntityType is not null)
{
  throw new InvalidOperationException(
    "V1456 LeashedEntity Remove did not retain its 7-bit slot.");
}

LeashedEntityModulePacket tracedLeashedKite = TerrariaPacketCodec.DecodeLeashedEntityModule(
  CreateLeashedEntityModuleFrame(LeashedEntityMessageType.FullSync, 4, writer =>
  {
    writer.Write7BitEncodedInt(1);
    writer.Write((short)100);
    writer.Write((short)-200);
    writer.Write7BitEncodedInt(88);
    writer.Write(1.25f);
    writer.Write(-2.5f);
    writer.Write(0x11223344u);
    writer.Write((byte)99);
    writer.Write(0.5f);
    writer.Write(0.75f);
    writer.Write(1.0f);
  }));
if (tracedLeashedKite.EntityType != 1 || tracedLeashedKite.AnchorX != 100 ||
    tracedLeashedKite.AnchorY != -200 || tracedLeashedKite.KiteState is not LeashedKiteStatePacket kite ||
    kite.ProjectileType != 88 || kite.PositionX != 1.25f || kite.PositionY != -2.5f ||
    kite.PackedVelocity != 0x11223344u || kite.Rotation != 99 || kite.WindTarget != 0.5f ||
    kite.CloudAlpha != 0.75f || kite.TimeCounter != 1.0f)
{
  throw new InvalidOperationException(
    "V1456 LeashedEntity Kite full sync did not retain its source fields.");
}

for (int entityType = 2; entityType <= 19; entityType++)
{
  LeashedEntityModulePacket packet = TerrariaPacketCodec.DecodeLeashedEntityModule(
    CreateLeashedEntityModuleFrame(LeashedEntityMessageType.PartialSync, 5, writer =>
    {
      writer.Write7BitEncodedInt(entityType);
      WriteLeashedCritterState(writer, full: false, extensionValue: null);
    }));
  if (packet.EntityType != entityType || packet.CritterState is null ||
      packet.CritterState.Value.NpcType is not null ||
      packet.CritterState.Value.PackedPositionOffset != 0xAABBCCDDu)
  {
    throw new InvalidOperationException(
      $"V1456 LeashedEntity partial critter type {entityType} did not retain its base state.");
  }
}

LeashedEntityModulePacket tracedButterfly = TerrariaPacketCodec.DecodeLeashedEntityModule(
  CreateLeashedEntityModuleFrame(LeashedEntityMessageType.FullSync, 6, writer =>
  {
    writer.Write7BitEncodedInt(7);
    writer.Write((short)1);
    writer.Write((short)2);
    WriteLeashedCritterState(writer, full: true, extensionValue: 4);
  }));
if (tracedButterfly.CritterState is not LeashedCritterStatePacket butterfly ||
    butterfly.NpcType != 55 || butterfly.Width != 16.0f || butterfly.Height != 24.0f ||
    butterfly.ExtensionKind != LeashedCritterExtensionKind.ButterflyVariant ||
    butterfly.ExtensionValue != 4)
{
  throw new InvalidOperationException(
    "V1456 LeashedEntity NormalButterfly full sync did not retain its variant suffix.");
}

LeashedEntityModulePacket tracedShimmerFly = TerrariaPacketCodec.DecodeLeashedEntityModule(
  CreateLeashedEntityModuleFrame(LeashedEntityMessageType.FullSync, 7, writer =>
  {
    writer.Write7BitEncodedInt(11);
    writer.Write((short)3);
    writer.Write((short)4);
    WriteLeashedCritterState(writer, full: true, extensionValue: 9);
  }));
if (tracedShimmerFly.CritterState is not LeashedCritterStatePacket shimmerFly ||
    shimmerFly.ExtensionKind != LeashedCritterExtensionKind.ShimmerFlyOldPositionsLength ||
    shimmerFly.ExtensionValue != 9)
{
  throw new InvalidOperationException(
    "V1456 LeashedEntity ShimmerFly full sync did not retain its suffix.");
}

try
{
  _ = TerrariaPacketCodec.DecodeLeashedEntityModule(
    CreateLeashedEntityModuleFrame(LeashedEntityMessageType.FullSync, 1, writer =>
    {
      writer.Write7BitEncodedInt(20);
    }));
  throw new InvalidOperationException("V1456 LeashedEntity accepted an unknown registry type.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeLeashedEntityModule(
    CreateLeashedEntityModuleFrame(LeashedEntityMessageType.FullSync, 1, writer =>
    {
      writer.Write7BitEncodedInt(7);
      writer.Write((short)1);
      writer.Write((short)2);
      WriteLeashedCritterState(writer, full: true, extensionValue: null);
    }));
  throw new InvalidOperationException(
    "V1456 LeashedEntity accepted a truncated NormalButterfly suffix.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeLeashedEntityModule(
    CreateLeashedEntityModuleFrame(LeashedEntityMessageType.Remove, 1, writer =>
    {
      writer.Write((byte)0);
    }));
  throw new InvalidOperationException("V1456 LeashedEntity Remove accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

TerrariaSession activeLeashedSession = CreateActiveSession(7);
NetModulePacket activeLeashed = activeLeashedSession.AcceptNetModule(
  CreateLeashedEntityModuleFrame(LeashedEntityMessageType.Remove, 300));
if (activeLeashed.ModuleId != 13 || activeLeashedSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 LeashedEntity did not preserve active-session compatibility handling.");
}

TerrariaSession activeTypedNetModulesSession = CreateActiveSession(7);
foreach (byte[] frameBytes in new byte[][]
{
  Convert.FromHexString("0D0052000001000A0014008003"),
  Convert.FromHexString("0D005202000000A03F000020C0"),
  Convert.FromHexString("0B00520300077856341209"),
  Convert.FromHexString("0A005206000734127856"),
  Convert.FromHexString("0B0052070002F6FF2A0008"),
  Convert.FromHexString("1B00520800040000A03F000020C000005040000070C00100000007"),
  Convert.FromHexString("090052090000341202"),
  Convert.FromHexString("0700520E000701")
})
{
  _ = activeTypedNetModulesSession.AcceptNetModule(frameBytes);
  if (activeTypedNetModulesSession.State != TerrariaSessionState.Active)
  {
    throw new InvalidOperationException(
      "A complete typed V1456 NetModule changed the active-session state.");
  }
}

CreativePowerModulePacket tracedSharedButton = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(1));
if (tracedSharedButton.PowerId != 1 ||
    tracedSharedButton.PayloadKind != CreativePowerPayloadKind.SharedButton)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers shared button did not retain its empty payload.");
}

CreativePowerModulePacket tracedSharedToggle = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(0, writer => writer.Write(true)));
if (tracedSharedToggle.PowerId != 0 ||
    tracedSharedToggle.PayloadKind != CreativePowerPayloadKind.SharedToggle ||
    tracedSharedToggle.ToggleState != true)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers shared toggle did not retain its Boolean payload.");
}

CreativePowerModulePacket tracedSharedSlider = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(6, writer => writer.Write(-0.75f)));
if (tracedSharedSlider.PowerId != 6 ||
    tracedSharedSlider.PayloadKind != CreativePowerPayloadKind.SharedSlider ||
    tracedSharedSlider.SliderValue != -0.75f)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers shared slider did not retain its Single payload.");
}

CreativePowerModulePacket tracedPerPlayerToggle = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(5, writer =>
  {
    writer.Write((byte)1);
    writer.Write((byte)7);
    writer.Write(true);
  }));
if (tracedPerPlayerToggle.PowerId != 5 ||
    tracedPerPlayerToggle.PayloadKind !=
      CreativePowerPayloadKind.PerPlayerToggleSyncOnePlayer ||
    tracedPerPlayerToggle.PlayerSlot != 7 || tracedPerPlayerToggle.ToggleState != true)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers per-player toggle did not retain its SyncOnePlayer payload.");
}

byte[] expectedPerPlayerToggleStates = new byte[32];
for (int index = 0; index < expectedPerPlayerToggleStates.Length; index++)
{
  expectedPerPlayerToggleStates[index] = (byte)index;
}

CreativePowerModulePacket tracedPerPlayerToggleEveryone =
  TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(11, writer =>
    {
      writer.Write((byte)0);
      writer.Write(expectedPerPlayerToggleStates);
    }));
if (tracedPerPlayerToggleEveryone.PowerId != 11 ||
    tracedPerPlayerToggleEveryone.PayloadKind !=
      CreativePowerPayloadKind.PerPlayerToggleSyncEveryone ||
    tracedPerPlayerToggleEveryone.PlayerStateBits is null ||
    !tracedPerPlayerToggleEveryone.PlayerStateBits.AsSpan().SequenceEqual(
      expectedPerPlayerToggleStates))
{
  throw new InvalidOperationException(
    "V1456 CreativePowers per-player toggle did not retain its SyncEveryone bitset.");
}

CreativePowerModulePacket tracedPerPlayerSlider = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(14, writer =>
  {
    writer.Write((byte)7);
    writer.Write(0.25f);
  }));
if (tracedPerPlayerSlider.PowerId != 14 ||
    tracedPerPlayerSlider.PayloadKind != CreativePowerPayloadKind.PerPlayerSlider ||
    tracedPerPlayerSlider.PlayerSlot != 7 || tracedPerPlayerSlider.SliderValue != 0.25f)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers per-player slider did not retain its payload.");
}

foreach (ushort powerId in new ushort[] { 2, 3, 4 })
{
  CreativePowerModulePacket packet = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(powerId));
  if (packet.PayloadKind != CreativePowerPayloadKind.SharedButton)
  {
    throw new InvalidOperationException(
      $"V1456 CreativePowers {powerId} did not retain its shared-button registration.");
  }
}

foreach (ushort powerId in new ushort[] { 7, 8, 12 })
{
  CreativePowerModulePacket packet = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(powerId, writer => writer.Write(0.5f)));
  if (packet.PayloadKind != CreativePowerPayloadKind.SharedSlider)
  {
    throw new InvalidOperationException(
      $"V1456 CreativePowers {powerId} did not retain its shared-slider registration.");
  }
}

foreach (ushort powerId in new ushort[] { 9, 10, 13 })
{
  CreativePowerModulePacket packet = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(powerId, writer => writer.Write(false)));
  if (packet.PayloadKind != CreativePowerPayloadKind.SharedToggle)
  {
    throw new InvalidOperationException(
      $"V1456 CreativePowers {powerId} did not retain its shared-toggle registration.");
  }
}

try
{
  _ = TerrariaPacketCodec.DecodeCreativePowerModule(CreateCreativePowerModuleFrame(15));
  throw new InvalidOperationException("V1456 CreativePowers accepted an unregistered power ID.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(5, writer => writer.Write((byte)2)));
  throw new InvalidOperationException("V1456 CreativePowers accepted an unknown toggle subtype.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(11, writer =>
    {
      writer.Write((byte)0);
      writer.Write(new byte[31]);
    }));
  throw new InvalidOperationException(
    "V1456 CreativePowers accepted a truncated SyncEveryone bitset.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeCreativePowerModule(
    CreateCreativePowerModuleFrame(1, writer => writer.Write((byte)0)));
  throw new InvalidOperationException("V1456 CreativePowers shared button accepted payload bytes.");
}
catch (InvalidDataException)
{
}

CreativePowerModulePacket nanSharedSlider = TerrariaPacketCodec.DecodeCreativePowerModule(
  CreateCreativePowerModuleFrame(7, writer => writer.Write(float.NaN)));
if (nanSharedSlider.SliderValue is not float sliderValue || !float.IsNaN(sliderValue))
{
  throw new InvalidOperationException(
    "V1456 CreativePowers did not preserve a source-readable NaN shared-slider value.");
}

TerrariaSession activeCreativePowersSession = CreateActiveSession(7);
NetModulePacket activeCreativePower = activeCreativePowersSession.AcceptNetModule(
  CreateCreativePowerModuleFrame(5, writer =>
  {
    writer.Write((byte)1);
    writer.Write((byte)42);
    writer.Write(true);
  }));
if (activeCreativePower.ModuleId != 5 ||
    activeCreativePowersSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 CreativePowers did not preserve active-session compatibility handling.");
}

JourneySpawnRatePacket activeJourneySpawnRate = TerrariaPacketCodec.DecodeJourneySpawnRate(
  CreateCreativePowerModuleFrame(14, writer =>
  {
    writer.Write((byte)7);
    writer.Write(0.25f);
  }));
if (activeJourneySpawnRate.PlayerSlot != 7 || activeJourneySpawnRate.SliderValue != 0.25f)
{
  throw new InvalidOperationException(
    "V1456 Journey spawn-rate lost its typed authoritative projection.");
}

try
{
  _ = activeCreativePowersSession.AcceptNetModule(
    CreateCreativePowerModuleFrame(14, writer =>
    {
      writer.Write((byte)8);
      writer.Write(0.25f);
    }));
  throw new InvalidOperationException(
    "V1456 Journey spawn-rate accepted a player slot not owned by the active session.");
}
catch (InvalidDataException)
{
}

byte[] clientTextFrame = CreateClientTextModuleFrame("Say", "hello from client");
ChatMessageModulePacket tracedClientText = TerrariaPacketCodec.DecodeClientTextModule(
  clientTextFrame);
if (tracedClientText.CommandId != "Say" || tracedClientText.Text != "hello from client")
{
  throw new InvalidOperationException(
    "V1456 NetModules Text C2S did not retain ChatMessage fields.");
}

TerrariaSession activeTextSession = CreateActiveSession(7);
NetModulePacket acceptedClientText = activeTextSession.AcceptNetModule(clientTextFrame);
if (acceptedClientText.ModuleId != 1 || activeTextSession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 NetModules Text C2S did not preserve active session state.");
}

byte[] serverTextFrame = CreateServerTextModuleFrame(
  0xFF,
  LegacyNetworkText.LocalizationKey(
    "Greeting.{0}",
    LegacyNetworkText.Literal("Alpha"),
    LegacyNetworkText.Formattable(
      "{0} Beta",
      LegacyNetworkText.LocalizationKey("Nested.Key"))),
  new TerrariaColor(12, 34, 56));
ServerTextModulePacket tracedServerText = TerrariaPacketCodec.DecodeServerTextModule(
  serverTextFrame);
if (tracedServerText.AuthorId != byte.MaxValue ||
    tracedServerText.Text.Mode != LegacyNetworkTextMode.LocalizationKey ||
    tracedServerText.Text.Text != "Greeting.{0}" ||
    tracedServerText.Text.Substitutions.Count != 2 ||
    tracedServerText.Text.Substitutions[0] != LegacyNetworkText.Literal("Alpha") ||
    tracedServerText.Text.Substitutions[1].Mode != LegacyNetworkTextMode.Formattable ||
    tracedServerText.Text.Substitutions[1].Substitutions[0].Text != "Nested.Key" ||
    tracedServerText.Color != new TerrariaColor(12, 34, 56))
{
  throw new InvalidOperationException(
    "V1456 NetModules Text S2C did not retain recursive NetworkText and RGB fields.");
}

try
{
  _ = TerrariaPacketCodec.DecodeClientTextModule(
    CreateClientTextModuleFrame("Say", "hello", new byte[] { 0x00 }));
  throw new InvalidOperationException("V1456 NetModules Text C2S accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeServerTextModule(
    CreateServerTextModuleFrame(1, null, new TerrariaColor(1, 2, 3), modeByte: 3));
  throw new InvalidOperationException("V1456 NetModules Text S2C accepted an unknown mode.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeServerTextModule(
    CreateServerTextModuleFrame(
      1,
      LegacyNetworkText.Literal("hello"),
      new TerrariaColor(1, 2, 3),
      trailingData: new byte[] { 0x00 }));
  throw new InvalidOperationException("V1456 NetModules Text S2C accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

NetPingModulePacket tracedNetPing = TerrariaPacketCodec.DecodeNetPingModule(
  Convert.FromHexString("0D005202000000A03F000020C0"));
if (tracedNetPing.PositionX != 1.25f || tracedNetPing.PositionY != -2.5f)
{
  throw new InvalidOperationException("V1456 NetModules Ping did not retain its Vector2 payload.");
}

IReadOnlyList<WorldLiquidSnapshot> tracedLiquidChanges = TerrariaPacketCodec.DecodeLiquidNetModule(
  Convert.FromHexString("0D0052000001000A0014008003"));
if (tracedLiquidChanges.Count != 1 || tracedLiquidChanges[0] != new WorldLiquidSnapshot(20, 10, 128, 3))
{
  throw new InvalidOperationException("V1456 NetModules Liquid did not retain its packed tile update.");
}

UnbreakableWallScanModulePacket tracedWallScan = TerrariaPacketCodec.DecodeUnbreakableWallScanModule(
  Convert.FromHexString("0700520E000701"));
if (tracedWallScan.PlayerSlot != 7 || !tracedWallScan.IsInsideUnbreakableWalls)
{
  throw new InvalidOperationException("V1456 NetModules wall scan did not retain its fixed payload.");
}

NetAmbienceModulePacket tracedAmbience = TerrariaPacketCodec.DecodeNetAmbienceModule(
  Convert.FromHexString("0B00520300077856341209"));
if (tracedAmbience.PlayerSlot != 7 || tracedAmbience.Seed != 0x12345678 ||
    tracedAmbience.SkyEntityType != 9)
{
  throw new InvalidOperationException("V1456 NetModules ambience did not retain its fixed payload.");
}

BestiaryModulePacket tracedBestiaryKill = TerrariaPacketCodec.DecodeBestiaryModule(
  Convert.FromHexString("0A00520400003412AC02"));
if (tracedBestiaryKill.UnlockType != BestiaryUnlockType.Kill ||
    tracedBestiaryKill.NpcNetId != 0x1234 || tracedBestiaryKill.KillCount != 300)
{
  throw new InvalidOperationException(
    "V1456 NetModules Bestiary Kill did not retain its 7-bit count payload.");
}

BestiaryModulePacket tracedBestiarySight = TerrariaPacketCodec.DecodeBestiaryModule(
  Convert.FromHexString("080052040001FEFF"));
if (tracedBestiarySight.UnlockType != BestiaryUnlockType.Sight ||
    tracedBestiarySight.NpcNetId != -2 || tracedBestiarySight.KillCount is not null)
{
  throw new InvalidOperationException(
    "V1456 NetModules Bestiary Sight did not retain its source-shaped payload.");
}

BestiaryModulePacket tracedBestiaryChat = TerrariaPacketCodec.DecodeBestiaryModule(
  Convert.FromHexString("0800520400027856"));
if (tracedBestiaryChat.UnlockType != BestiaryUnlockType.Chat ||
    tracedBestiaryChat.NpcNetId != 0x5678 || tracedBestiaryChat.KillCount is not null)
{
  throw new InvalidOperationException(
    "V1456 NetModules Bestiary Chat did not retain its source-shaped payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeBestiaryModule(Convert.FromHexString("080052040001FEFF00"));
  throw new InvalidOperationException(
    "V1456 NetModules Bestiary Sight accepted trailing payload data.");
}
catch (InvalidDataException)
{
}

try
{
  _ = TerrariaPacketCodec.DecodeBestiaryModule(Convert.FromHexString("080052040003FEFF"));
  throw new InvalidOperationException("V1456 NetModules Bestiary accepted an unknown type.");
}
catch (InvalidDataException)
{
}

TerrariaSession activeBestiarySession = CreateActiveSession(7);
NetModulePacket activeBestiary = activeBestiarySession.AcceptNetModule(
  Convert.FromHexString("0A00520400003412AC02"));
if (activeBestiary.ModuleId != 4 || activeBestiarySession.State != TerrariaSessionState.Active)
{
  throw new InvalidOperationException(
    "V1456 NetModules Bestiary did not preserve the active session state.");
}

try
{
  _ = activeBestiarySession.AcceptNetModule(Convert.FromHexString("080052040003FEFF"));
  throw new InvalidOperationException(
    "The active NetModules session accepted an unknown Bestiary type.");
}
catch (InvalidDataException)
{
}

CreativePowerPermissionModulePacket tracedPermission =
  TerrariaPacketCodec.DecodeCreativePowerPermissionModule(
    Convert.FromHexString("090052090000341202"));
if (tracedPermission.PowerId != 0x1234 || tracedPermission.PermissionLevel != 2)
{
  throw new InvalidOperationException(
    "V1456 NetModules power permission did not retain its payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeCreativePowerPermissionModule(
    Convert.FromHexString("090052090001341202"));
  throw new InvalidOperationException(
    "V1456 NetModules power permission accepted an unsupported selector.");
}
catch (InvalidDataException)
{
}

CreativeUnlocksPlayerReportModulePacket tracedCreativeUnlocks =
  TerrariaPacketCodec.DecodeCreativeUnlocksPlayerReportModule(
    Convert.FromHexString("0A005206000734127856"));
if (tracedCreativeUnlocks.ReportedPlayerSlot != 7 ||
    tracedCreativeUnlocks.ItemId != 0x1234 || tracedCreativeUnlocks.Amount != 0x5678)
{
  throw new InvalidOperationException(
    "V1456 NetModules creative unlock report did not retain its fixed payload.");
}

TeleportPylonModulePacket tracedTeleportPylon =
  TerrariaPacketCodec.DecodeTeleportPylonModule(
    Convert.FromHexString("0B0052070002F6FF2A0008"));
if (tracedTeleportPylon.SubPacketType != 2 || tracedTeleportPylon.TileX != -10 ||
    tracedTeleportPylon.TileY != 42 || tracedTeleportPylon.PylonType != 8)
{
  throw new InvalidOperationException(
    "V1456 NetModules pylon did not retain its fixed payload.");
}

try
{
  _ = TerrariaPacketCodec.DecodeTeleportPylonModule(
    Convert.FromHexString("0B0052070003F6FF2A0008"));
  throw new InvalidOperationException(
    "V1456 NetModules pylon accepted an unsupported selector.");
}
catch (InvalidDataException)
{
}

ParticleOrchestraModulePacket tracedParticles =
  TerrariaPacketCodec.DecodeParticleOrchestraModule(
    Convert.FromHexString("1B00520800040000A03F000020C000005040000070C00100000007"));
if (tracedParticles.ParticleType != 4 ||
    tracedParticles.Position != new SimulationVector(1.25f, -2.5f) ||
    tracedParticles.Movement != new SimulationVector(3.25f, -3.75f) ||
    tracedParticles.UniqueInfoPiece != 1 || tracedParticles.InvokingPlayerSlot != 7)
{
  throw new InvalidOperationException(
    "V1456 NetModules particles did not retain its fixed payload.");
}

byte[] tracedClientProjectile = Convert.FromHexString(
  "32001B010000000000000000000000000000000000010100FF010000803F00000040030004000000A0400600070000000041");
if (TerrariaPacketCodec.DecodeClientProjectileOwner(tracedClientProjectile) != 1)
{
  throw new InvalidOperationException("V1456 SyncProjectile raw frame did not retain its owner.");
}

SignOpenRequestPacket tracedSignRequest = TerrariaPacketCodec.DecodeSignOpenRequest(
  Convert.FromHexString("07002E0A000B00"));
if (tracedSignRequest.TileX != 10 || tracedSignRequest.TileY != 11)
{
  throw new InvalidOperationException("V1456 OpenSignRequest raw frame did not retain its fields.");
}

SignUpdateIntent tracedSignUpdate = TerrariaPacketCodec.DecodeSignUpdate(
  Convert.FromHexString("0E002F01000A000B000248690100"));
if (tracedSignUpdate.PlayerSlot != 1 || tracedSignUpdate.SignId != 1 ||
    tracedSignUpdate.TileX != 10 || tracedSignUpdate.TileY != 11 ||
    tracedSignUpdate.Text != "Hi" || tracedSignUpdate.SuppressOpenSign)
{
  throw new InvalidOperationException("V1456 OpenSignResponse raw frame did not retain its fields.");
}

if (!TerrariaPacketCodec.EncodeSignState(new SignReplicationSnapshot(
  1,
  10,
  11,
  "Hi",
  1,
  false,
  1)).AsSpan().SequenceEqual(Convert.FromHexString("0E002F01000A000B000248690100")))
{
  throw new InvalidOperationException("V1456 OpenSignResponse did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodePlayerActive(4, true).AsSpan()
  .SequenceEqual(Convert.FromHexString("05000E0401")))
{
  throw new InvalidOperationException("V1456 PlayerActive did not retain its source wire form.");
}

DoorToggleIntent tracedDoorOpen = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013000A000A0001"));
if (tracedDoorOpen.Action != DoorToggleAction.OpenDoor || tracedDoorOpen.TileX != 10 ||
    tracedDoorOpen.TileY != 10 || !tracedDoorOpen.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedDoorOpen).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013000A000A0001")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState open action did not retain its source wire form.");
}

DoorToggleIntent tracedDoorClose = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013010A000A0000"));
if (tracedDoorClose.Action != DoorToggleAction.CloseDoor || tracedDoorClose.TileX != 10 ||
    tracedDoorClose.TileY != 10 || tracedDoorClose.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedDoorClose).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013010A000A0000")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState close action did not retain its source wire form.");
}

DoorToggleIntent tracedTrapdoor = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013020A000A0001"));
if (tracedTrapdoor.Action != DoorToggleAction.ShiftTrapdoor || tracedTrapdoor.TileX != 10 ||
    tracedTrapdoor.TileY != 10 || !tracedTrapdoor.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedTrapdoor).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013020A000A0001")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState trapdoor action did not retain its source wire form.");
}

DoorToggleIntent tracedTrapdoorReverse = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013030A000A0000"));
if (tracedTrapdoorReverse.Action != DoorToggleAction.ShiftTrapdoorReverse ||
    tracedTrapdoorReverse.TileX != 10 || tracedTrapdoorReverse.TileY != 10 ||
    tracedTrapdoorReverse.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedTrapdoorReverse).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013030A000A0000")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState reverse trapdoor action did not retain its source wire form.");
}

DoorToggleIntent tracedTallGateOpen = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013040A000A0001"));
if (tracedTallGateOpen.Action != DoorToggleAction.OpenTallGate ||
    tracedTallGateOpen.TileX != 10 || tracedTallGateOpen.TileY != 10 ||
    !tracedTallGateOpen.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedTallGateOpen).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013040A000A0001")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState tall-gate open action did not retain its source wire form.");
}

DoorToggleIntent tracedTallGateClose = TerrariaPacketCodec.DecodeDoorToggle(
  Convert.FromHexString("090013050A000A0000"));
if (tracedTallGateClose.Action != DoorToggleAction.CloseTallGate ||
    tracedTallGateClose.TileX != 10 || tracedTallGateClose.TileY != 10 ||
    tracedTallGateClose.Direction ||
    !TerrariaPacketCodec.EncodeDoorToggle(tracedTallGateClose).AsSpan()
      .SequenceEqual(Convert.FromHexString("090013050A000A0000")))
{
  throw new InvalidOperationException(
    "V1456 ToggleDoorState tall-gate close action did not retain its source wire form.");
}

byte[] tracedStatusText = Convert.FromHexString(
  "1D00090F0000000013526563656976696E672074696C65206461746100");
if (!TerrariaPacketCodec.EncodeStatusTextSize(15, "Receiving tile data").AsSpan()
  .SequenceEqual(tracedStatusText))
{
  throw new InvalidOperationException(
    "V1456 StatusTextSize literal NetworkText did not retain its source wire form.");
}

LegacyNetworkText nestedStatusText = LegacyNetworkText.LocalizationKey(
  "Legacy.Key",
  [
    LegacyNetworkText.Literal("Alpha"),
    LegacyNetworkText.Formattable(
      "{0}:{1}",
      [LegacyNetworkText.LocalizationKey("Nested.Key")])
  ]);
byte[] tracedNestedStatusText = Convert.FromHexString(
  string.Concat(
    "3300090F000000020A4C65676163792E4B6579020005416C706861",
    "01077B307D3A7B317D01020A4E65737465642E4B657900A5"));
if (!TerrariaPacketCodec.EncodeStatusTextSize(15, nestedStatusText, 0xA5).AsSpan()
  .SequenceEqual(tracedNestedStatusText))
{
  throw new InvalidOperationException(
    "V1456 StatusTextSize did not retain recursive NetworkText source wire form.");
}

byte[] tracedChestSize = Convert.FromHexString("07009B01002800");
if (!TerrariaPacketCodec.EncodeChestSize(1, 40).AsSpan().SequenceEqual(tracedChestSize))
{
  throw new InvalidOperationException("V1456 SyncChestSize did not retain its source wire form.");
}

byte[] tracedChestItem = Convert.FromHexString("0B00200100000100000100");
if (!TerrariaPacketCodec.EncodeChestItem(new ChestItemReplicationSnapshot(
  1,
  0,
  new ItemStack(1, 1),
  null,
  0)).AsSpan().SequenceEqual(tracedChestItem))
{
  throw new InvalidOperationException("V1456 SyncChestItem did not retain its source wire form.");
}

bool rejectedNonCanonicalChestItem = false;
try
{
  _ = TerrariaPacketCodec.EncodeChestItem(new ChestItemReplicationSnapshot(
    1,
    0,
    new ItemStack(0, 1),
    null,
    0));
}
catch (ArgumentOutOfRangeException)
{
  rejectedNonCanonicalChestItem = true;
}

if (!rejectedNonCanonicalChestItem)
{
  throw new InvalidOperationException(
    "V1456 SyncChestItem accepted a non-canonical empty ItemStack.");
}

byte[] tracedWorldItem = Convert.FromHexString(
  "1B0015010000008041000080410000000000000000010000010100");
if (!TerrariaPacketCodec.EncodeItemReplication(new ItemReplicationSnapshot(
  1,
  new ItemStack(1, 1),
  new SimulationVector(1.0f, 1.0f),
  true,
  1,
  new WorldSectionCoordinates(0, 0))).AsSpan().SequenceEqual(tracedWorldItem))
{
  throw new InvalidOperationException("V1456 SyncItem did not retain its source wire form.");
}

bool rejectedInvalidNetworkItemProjection = false;
try
{
  NetworkItemSlice.From(new ItemReplicationSnapshot(
    1,
    ItemStack.Empty,
    new SimulationVector(1.0f, 1.0f),
    IsActive: true,
    Revision: 1,
    new WorldSectionCoordinates(0, 0)));
}
catch (ArgumentOutOfRangeException)
{
  rejectedInvalidNetworkItemProjection = true;
}

if (!rejectedInvalidNetworkItemProjection)
{
  throw new InvalidOperationException(
    "Network item isolation projected an invalid active/empty replication snapshot.");
}

byte[] tracedProjectileDespawn = Convert.FromHexString("06001D010001");
ProjectileReplicationSnapshot despawnedProjectile = new(
  ReplicationId: 1,
  ProjectileType: 1,
  Owner: new PlayerHandle(1),
  Position: new SimulationVector(0.0f, 0.0f),
  Velocity: new SimulationVector(0.0f, 0.0f),
  Damage: 0,
  RemainingLifetime: 0,
  IsActive: false,
  Revision: 1,
  Section: new WorldSectionCoordinates(0, 0));
if (!TerrariaPacketCodec.EncodeProjectileDespawn(despawnedProjectile).AsSpan()
  .SequenceEqual(tracedProjectileDespawn))
{
  throw new InvalidOperationException(
    "V1456 KillProjectile did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeWorldBiomeTypes(WorldJoinStateSnapshot.CreateDefault()).AsSpan()
  .SequenceEqual(Convert.FromHexString("060039000600")))
{
  throw new InvalidOperationException("V1456 WorldBiomeTypes did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeTowerShieldStrengths(WorldJoinStateSnapshot.CreateDefault())
  .AsSpan().SequenceEqual(Convert.FromHexString("0B00650000000000000000")))
{
  throw new InvalidOperationException(
    "V1456 TowerShieldStrengths did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeCavernMonsterTypes(WorldJoinStateSnapshot.CreateDefault())
  .AsSpan().SequenceEqual(Convert.FromHexString("0F0088F701F201F901F101F001F001")))
{
  throw new InvalidOperationException(
    "V1456 CavernMonsterTypes did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeAnglerQuest(WorldJoinStateSnapshot.CreateDefault()).AsSpan()
  .SequenceEqual(Convert.FromHexString("05004A1400")))
{
  throw new InvalidOperationException("V1456 AnglerQuest did not retain its source wire form.");
}

AnglerQuestPacket anglerQuest = new(42, FinishedToday: true);
if (TerrariaPacketCodec.DecodeAnglerQuest(TerrariaPacketCodec.EncodeAnglerQuest(anglerQuest)) !=
    anglerQuest ||
    TerrariaMessageCatalog.Get(TerrariaMessageId.AnglerQuest).Support !=
      TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("V1456 AnglerQuest did not preserve its typed fields.");
}

try
{
  _ = TerrariaPacketCodec.DecodeAnglerQuest(
    TerrariaPacketCodec.EncodeAnglerQuest(anglerQuest)[..^1]);
  throw new InvalidOperationException("V1456 AnglerQuest accepted a truncated frame.");
}
catch (InvalidDataException)
{
}

if (!TerrariaPacketCodec.EncodeNpcHome(new NpcHomeSnapshot(1, 2152, 300, false)).AsSpan()
  .SequenceEqual(Convert.FromHexString("0A003C010068082C0100")))
{
  throw new InvalidOperationException("V1456 NpcHome did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeNpcHome(
  new NpcHomeSnapshot(1, 2152, 300, false),
  homeState: 2).AsSpan().SequenceEqual(Convert.FromHexString("0A003C010068082C0102")))
{
  throw new InvalidOperationException("V1456 NpcHome room assignment did not retain its source wire form.");
}

if (!TerrariaPacketCodec.EncodeHostStatus(1, true).AsSpan()
  .SequenceEqual(Convert.FromHexString("05008B0101")))
{
  throw new InvalidOperationException("V1456 HostStatus did not retain its source wire form.");
}

byte[] tracedPlayerProfile = Convert.FromHexString(
  "36000401000100000000000E446F6D654175746F6D6174696F6E00000000" +
  "D75A37FF7D5A695A4BAFA58CA0B4D7FFE6AFA0693C001000");
PlayerProfilePacket tracedPlayerProfilePacket = TerrariaPacketCodec.DecodePlayerProfile(
  tracedPlayerProfile);
if (tracedPlayerProfilePacket.PlayerSlot != 1 ||
    tracedPlayerProfilePacket.Name != "DomeAutomation" ||
    tracedPlayerProfilePacket.AccessoryVisibility != 0 ||
    tracedPlayerProfilePacket.DifficultyFlags != 0 ||
    tracedPlayerProfilePacket.BiomeTorchFlags != (1 << 4) ||
    tracedPlayerProfilePacket.ConsumableFlags != 0)
{
  throw new InvalidOperationException(
    "V1456 SyncPlayer raw frame did not retain its source fields.");
}

PlayerUuidPacket tracedPlayerUuid = TerrariaPacketCodec.DecodePlayerUuid(Convert.FromHexString(
  "2800442431323262396530652D353032312D346363652D626264312D626434333566653934323665"));
if (tracedPlayerUuid.Value != "122b9e0e-5021-4cce-bbd1-bd435fe9426e")
{
  throw new InvalidOperationException("V1456 PlayerUuid raw frame did not retain its UUID string.");
}

PlayerVitalsPacket tracedPlayerLifeMana = TerrariaPacketCodec.DecodePlayerLifeMana(
  Convert.FromHexString("0800100164006400"));
if (tracedPlayerLifeMana.PlayerSlot != 1 || tracedPlayerLifeMana.Current != 100 ||
    tracedPlayerLifeMana.Maximum != 100)
{
  throw new InvalidOperationException("V1456 PlayerLifeMana raw frame did not retain its vitals.");
}

PlayerVitalsPacket tracedPlayerMana = TerrariaPacketCodec.DecodePlayerMana(
  Convert.FromHexString("08002A0100001400"));
if (tracedPlayerMana.PlayerSlot != 1 || tracedPlayerMana.Current != 0 ||
    tracedPlayerMana.Maximum != 20)
{
  throw new InvalidOperationException(
    "V1456 ItemRotationAndAnimation raw frame did not retain its mana values.");
}

PlayerBuffsPacket tracedPlayerBuffs = TerrariaPacketCodec.DecodePlayerBuffs(
  Convert.FromHexString("060032010000"));
if (tracedPlayerBuffs.PlayerSlot != 1 || tracedPlayerBuffs.BuffTypes.Count != 0)
{
  throw new InvalidOperationException("V1456 PlayerBuffs raw frame did not retain its terminator.");
}

PlayerLoadoutPacket tracedPlayerLoadout = TerrariaPacketCodec.DecodePlayerLoadout(
  Convert.FromHexString("07009301000000"));
if (tracedPlayerLoadout.PlayerSlot != 1 || tracedPlayerLoadout.SelectedLoadout != 0 ||
    tracedPlayerLoadout.AccessoryVisibility != 0)
{
  throw new InvalidOperationException("V1456 SyncLoadout raw frame did not retain its fields.");
}

PlayerEquipmentPacket tracedPlayerEquipment = TerrariaPacketCodec.DecodePlayerEquipment(
  Convert.FromHexString("0C0005010000000000000000"));
if (tracedPlayerEquipment.PlayerSlot != 1 || tracedPlayerEquipment.SlotId != 0 ||
    tracedPlayerEquipment.Stack != 0 || tracedPlayerEquipment.Prefix != 0 ||
    tracedPlayerEquipment.ItemType != 0 || tracedPlayerEquipment.IsFavorited ||
    tracedPlayerEquipment.IsNewAndShiny)
{
  throw new InvalidOperationException("V1456 SyncEquipment raw frame did not retain its fields.");
}

byte[] domeWorldData = TerrariaV1456Compatibility.EncodeWorldData(domeContext);
if (domeWorldData.Length != 174)
{
  throw new InvalidOperationException("Dome WorldData no longer has its projected V1456 length.");
}

LegacyWorldDataContext invasionFlagsContext = domeContext.WithWorldState(
  time: 0,
  isDayTime: true,
  moonPhase: 0,
  progression: new WorldProgressionState(
    defeatedGoblins: true,
    defeatedFrost: true,
    defeatedPirates: true,
    defeatedMartians: true),
  rules: new WorldRuleState());
if ((invasionFlagsContext.Progression.EventFlags8 & (1 << 6)) == 0 ||
    (invasionFlagsContext.Progression.EventFlags10 & 1) == 0 ||
    (invasionFlagsContext.Progression.EventFlags10 & (1 << 1)) == 0 ||
    (invasionFlagsContext.Progression.EventFlags10 & (1 << 2)) == 0 ||
    TerrariaV1456Compatibility.EncodeWorldData(invasionFlagsContext).Length != 174)
{
  throw new InvalidOperationException(
    "V1456 WorldData did not project the named invasion clear flags into source bits.");
}

LegacyWorldDataContext moonPhaseContext = domeContext.WithWorldState(
  time: 0,
  isDayTime: true,
  moonPhase: 4,
  progression: new WorldProgressionState(),
  rules: new WorldRuleState());
TerrariaFrame moonPhaseFrame = TerrariaFrameCodec.Decode(
  TerrariaV1456Compatibility.EncodeWorldData(moonPhaseContext));
if (moonPhaseFrame.MessageId != TerrariaMessageId.WorldData ||
    moonPhaseFrame.Payload.Span[5] != 4)
{
  throw new InvalidOperationException("V1456 WorldData did not preserve its projected moon phase.");
}

LegacyWorldDataContext traceNameContext = domeContext with { WorldName = "FullServerTrace" };
byte[] traceNameWorldData = TerrariaV1456Compatibility.EncodeWorldData(traceNameContext);
if (traceNameWorldData.Length != 179)
{
  throw new InvalidOperationException(
    "V1456 WorldData must account for the authoritative world name length.");
}

LegacyWorldBackgroundState rawWorldBackground = new(
  1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
  1.5f, 17, 101, 202, 303, 18, 19, 20, 21, 404, 505, 606, 22, 23, 24, 25,
  26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 2.5f);
LegacyWorldProgressionState rawWorldProgression = new(
  0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
  0x3C, 0x3D, -5, 0x1112131415161718UL, 3.5f);
LegacyOreTierState rawWorldOreTiers = new(1001, 1002, 1003, 1004, 1005, 1006, 1007);
LegacyWorldDataContext rawWorldDataContext = new(
  0x11223344,
  0xE5,
  7,
  4000,
  1200,
  2000,
  300,
  250,
  700,
  0x55667788,
  "A",
  2,
  new Guid("00112233-4455-6677-8899-aabbccddeeff"),
  0x0102030405060708UL,
  3,
  rawWorldBackground,
  rawWorldProgression,
  rawWorldOreTiers,
  new LegacySpawnPointSet(
    [new LegacySpawnPoint(700, 800), new LegacySpawnPoint(-3, 900)]),
  IsNoTrapsWorld: true,
  IsSkyblockWorld: true);
byte[] tracedWorldData = Convert.FromHexString(string.Concat(
  "AD000744332211E507A00FB004D0072C01FA00BC0288776655014102",
  "33221100554477668899AABBCCDDEEFF080706050403020103",
  "0102030405060708090A0B0C0D0E0F100000C03F1165000000CA0000002F010000",
  "1213141594010000F90100005E020000161718191A1B1C1D1E1F20212223242526",
  "000020403132333435363738393A3B413C3DE903EA03EB03EC03ED03EE03EF03FB",
  "18171615141312110000604002BC022003FDFF8403"));
if (!TerrariaV1456Compatibility.EncodeWorldData(rawWorldDataContext).AsSpan()
  .SequenceEqual(tracedWorldData))
{
  throw new InvalidOperationException("V1456 WorldData did not retain the complete source field order.");
}

IReadOnlyList<byte[]> greetings = TerrariaV1456Compatibility.CreateJoinGreetingFrames(
  domeContext,
  "DomeAutomation");
if (greetings.Count != 2 || greetings[0].Length != 55 || greetings[1].Length != 45)
{
  throw new InvalidOperationException(
    "V1456 join greeting must retain the original NetTextModule field lengths.");
}

WorldGrid world = new(width: 400, height: 300);
_ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
byte[] tileSquare = TerrariaV1456Compatibility.EncodeTileSquare(
  world,
  x: 10,
  y: 10,
  width: 1,
  height: 1,
  changeType: 0);
TerrariaFrame tileSquareFrame = TerrariaFrameCodec.Decode(tileSquare);
if (tileSquareFrame.Payload.Length != 12 ||
    tileSquareFrame.Payload.Span[7] != 1 ||
    tileSquareFrame.Payload.Span[8] != 0 ||
    tileSquareFrame.Payload.Span[9] != 0)
{
  throw new InvalidOperationException("V1456 TileSquare did not retain the original three flag bytes.");
}

byte[] exploitDestroyTileSquare = TerrariaPacketCodec.EncodeExploitDestroyTileSquare(
  world,
  new ExploitDestroyTileSquareIntent(10, 10));
TerrariaFrame exploitDestroyTileSquareFrame = TerrariaFrameCodec.Decode(exploitDestroyTileSquare);
if (exploitDestroyTileSquareFrame.MessageId != TerrariaMessageId.TileSquare ||
    !exploitDestroyTileSquare.AsSpan().SequenceEqual(tileSquare))
{
  throw new InvalidOperationException(
    "V1456 exploit-destroy projection did not preserve the broadcast tile-square payload.");
}

try
{
  _ = TerrariaPacketCodec.EncodeExploitDestroyTileSquare(
    world,
    new ExploitDestroyTileSquareIntent(10, 10, RemoteClient: 1));
  throw new InvalidOperationException("V1456 exploit-destroy projection accepted a directed client.");
}
catch (ArgumentOutOfRangeException)
{
}

LegacyTileSquareTile tracedTileSquareTile = new(
  IsActive: true,
  HasWall: true,
  HasLiquid: true,
  HasWire: true,
  IsHalfBrick: true,
  HasActuator: true,
  IsInactive: true,
  HasWire2: true,
  HasWire3: true,
  TileColor: 0x20,
  WallColor: 0x21,
  Slope: 5,
  HasWire4: true,
  IsFullBrightBlock: true,
  IsFullBrightWall: true,
  IsInvisibleBlock: true,
  IsInvisibleWall: true,
  TileType: 0x0123,
  IsFrameImportant: true,
  FrameX: 0x0102,
  FrameY: 0x0304,
  WallType: 0x0456,
  LiquidAmount: 0x78,
  LiquidType: 2);
byte[] tracedTileSquare = Convert.FromHexString(
  "1900140A001400010102FDDF0F202123010201040356047802");
if (!TerrariaV1456Compatibility.EncodeTileSquare(
  10,
  20,
  1,
  1,
  2,
  [tracedTileSquareTile]).AsSpan().SequenceEqual(tracedTileSquare))
{
  throw new InvalidOperationException(
    "V1456 TileSquare did not retain all source-controlled conditional fields.");
}

NpcReplicationSnapshot snapshot = new(
  ReplicationId: 4,
  NpcType: 1,
  Position: new SimulationVector(10, 20),
  Velocity: new SimulationVector(0, 0),
  Health: 75,
  IsActive: true,
  Revision: 1,
  Section: new WorldSectionCoordinates(0, 0));
LegacyNpcWireState npcState = LegacyNpcWireState.CreateDefault(snapshot) with
{
  Ai0 = 1.0f,
  DirectionRight = true,
  LifeMaximum = 100
};
byte[] npcFrame = TerrariaV1456Compatibility.EncodeNpcReplication(npcState);
if (npcFrame.Length != 33)
{
  throw new InvalidOperationException(
    "V1456 SyncNPC must include present AI and original compact life fields.");
}

LegacyNpcWireState maximalNpcState = npcState with
{
  Target = 5,
  DirectionYDown = true,
  Ai1 = 2.0f,
  Ai2 = 3.0f,
  Ai3 = 4.0f,
  SpriteDirectionRight = true,
  PlayersForScaling = 2,
  SpawnedFromStatue = true,
  Difficulty = 2.0f,
  SpawnNeedsSyncing = true,
  ShimmerTransparency = 1.0f,
  ReleaseOwner = 8,
  IsCatchable = true
};
if (!TerrariaV1456Compatibility.EncodeNpcReplication(maximalNpcState).AsSpan()
  .SequenceEqual(Convert.FromHexString(
    "3300170400000020430000A043000000000000000005007F1F0000803F00000040" +
    "000040400000804001000200000040014B08")))
{
  throw new InvalidOperationException("V1456 SyncNPC did not retain its maximal source wire form.");
}

if (!TerrariaPacketCodec.EncodeNpcBuffs(snapshot with { ReplicationId = 1 }).AsSpan()
  .SequenceEqual(Convert.FromHexString("07003601000000")))
{
  throw new InvalidOperationException("V1456 NpcBuffs did not retain its empty source wire form.");
}

if (!TerrariaPacketCodec.EncodeNpcBuffs(
  snapshot with { ReplicationId = 1 },
  [new NpcBuffEntry(2, 3)]).AsSpan()
  .SequenceEqual(Convert.FromHexString("0B00360100020003000000")))
{
  throw new InvalidOperationException("V1456 NpcBuffs did not retain its nonempty source wire form.");
}

byte[] mountPlayerControls = CreatePlayerControlsFrame(
  playerSlot: 7,
  flags1: (1 << 3) | (1 << 1),
  flags2: 1 << 7,
  flags3: 0,
  flags4: 0,
  selectedItem: 4,
  positionX: 123.5f,
  positionY: 456.25f,
  mountType: 1);
TerrariaFrame mountPlayerControlsFrame = TerrariaFrameCodec.Decode(mountPlayerControls);
if (mountPlayerControlsFrame.Payload.Length != 16)
{
  throw new InvalidOperationException("V1456 mount PlayerControls payload must be 16 bytes.");
}

PlayerControlIntent mountControls = TerrariaPacketCodec.DecodePlayerControls(mountPlayerControls);
if (mountControls.PlayerSlot != 7 || !mountControls.MoveRight || !mountControls.Down ||
    mountControls.SelectedItem != 4)
{
  throw new InvalidOperationException("V1456 mount PlayerControls fixed fields were not retained.");
}

byte[] downPlayerControls = TerrariaPacketCodec.EncodePlayerControls(new PlayerControlIntent(
  7,
  MoveLeft: false,
  MoveRight: false,
  Jump: false,
  UseItem: false,
  FacingRight: true,
  SelectedItem: 4,
  Down: true),
  positionX: 123.5f,
  positionY: 456.25f);
TerrariaFrame downPlayerControlsFrame = TerrariaFrameCodec.Decode(downPlayerControls);
if (downPlayerControlsFrame.Payload.Length != 14 ||
    (downPlayerControlsFrame.Payload.Span[1] & (1 << 1)) == 0 ||
    !TerrariaPacketCodec.DecodePlayerControls(downPlayerControls).Down)
{
  throw new InvalidOperationException("V1456 PlayerControls did not retain controlDown at bit 1.");
}

LegacyPlayerControlsProjection mountProjection =
  TerrariaPacketCodec.DecodePlayerControlsCompatibility(mountPlayerControls);
if (mountProjection.State.MountType != 1 || mountProjection.State.Velocity is not null ||
    mountProjection.State.ReturnOrigin is not null || mountProjection.State.CameraTarget is not null)
{
  throw new InvalidOperationException("V1456 mount PlayerControls suffix was not projected.");
}

VerifyPlayerControlsProjection(
  "fixed",
  CreatePlayerControlsFrame(
    7,
    0,
    0,
    0,
    0,
    4,
    123.5f,
    456.25f),
  expectedPayloadLength: 14,
  expectedVelocity: null,
  expectedMountType: null,
  expectedReturnOrigin: null,
  expectedReturnHome: null,
  expectedCameraTarget: null);

SimulationVector velocity = new(11.0f, 12.0f);
VerifyPlayerControlsProjection(
  "velocity",
  CreatePlayerControlsFrame(
    7,
    0,
    1 << 2,
    0,
    0,
    4,
    123.5f,
    456.25f,
    velocity: velocity),
  expectedPayloadLength: 22,
  expectedVelocity: velocity,
  expectedMountType: null,
  expectedReturnOrigin: null,
  expectedReturnHome: null,
  expectedCameraTarget: null);

SimulationVector returnOrigin = new(21.0f, 22.0f);
SimulationVector returnHome = new(23.0f, 24.0f);
VerifyPlayerControlsProjection(
  "return",
  CreatePlayerControlsFrame(
    7,
    0,
    0,
    1 << 6,
    0,
    4,
    123.5f,
    456.25f,
    returnOrigin: returnOrigin,
    returnHome: returnHome),
  expectedPayloadLength: 30,
  expectedVelocity: null,
  expectedMountType: null,
  expectedReturnOrigin: returnOrigin,
  expectedReturnHome: returnHome,
  expectedCameraTarget: null);

SimulationVector cameraTarget = new(31.0f, 32.0f);
VerifyPlayerControlsProjection(
  "camera",
  CreatePlayerControlsFrame(
    7,
    0,
    0,
    0,
    1 << 5,
    4,
    123.5f,
    456.25f,
    cameraTarget: cameraTarget),
  expectedPayloadLength: 22,
  expectedVelocity: null,
  expectedMountType: null,
  expectedReturnOrigin: null,
  expectedReturnHome: null,
  expectedCameraTarget: cameraTarget);

SimulationVector maximumVelocity = new(41.0f, 42.0f);
SimulationVector maximumReturnOrigin = new(43.0f, 44.0f);
SimulationVector maximumReturnHome = new(45.0f, 46.0f);
SimulationVector maximumCameraTarget = new(47.0f, 48.0f);
VerifyPlayerControlsProjection(
  "maximum",
  CreatePlayerControlsFrame(
    7,
    0,
    (byte)((1 << 2) | (1 << 7)),
    1 << 6,
    1 << 5,
    4,
    123.5f,
    456.25f,
    maximumVelocity,
    2,
    maximumReturnOrigin,
    maximumReturnHome,
    maximumCameraTarget),
  expectedPayloadLength: 48,
  expectedVelocity: maximumVelocity,
  expectedMountType: 2,
  expectedReturnOrigin: maximumReturnOrigin,
  expectedReturnHome: maximumReturnHome,
  expectedCameraTarget: maximumCameraTarget);

AssertTruncatedPlayerControlsSuffix(
  CreatePlayerControlsFrame(7, 0, 1 << 7, 0, 0, 4, 0.0f, 0.0f,
    trailingData: [1]),
  "truncated mount suffix");
AssertTruncatedPlayerControlsSuffix(
  CreatePlayerControlsFrame(7, 0, 0, 1 << 6, 0, 4, 0.0f, 0.0f,
    trailingData: new byte[15]),
  "truncated return-position suffix");
AssertTruncatedPlayerControlsSuffix(
  CreatePlayerControlsFrame(7, 0, 0, 0, 1 << 5, 4, 0.0f, 0.0f,
    trailingData: new byte[7]),
  "truncated camera-target suffix");

LegacyTileSectionTile sourceCompleteTile = new(
  IsActive: true,
  HasWall: true,
  LiquidAmount: 0x11,
  LiquidKind: LegacyTileSectionLiquidKind.Shimmer,
  HasWire1: true,
  HasWire2: true,
  HasWire3: true,
  HasWire4: true,
  IsHalfBrick: false,
  Slope: 3,
  HasActuator: true,
  IsInactive: true,
  TileColor: 0xA0,
  WallColor: 0xF0,
  IsInvisibleBlock: true,
  IsInvisibleWall: true,
  IsFullBrightBlock: true,
  IsFullBrightWall: true,
  TileType: 0x1234,
  IsFrameImportant: true,
  FrameX: 0x5678,
  FrameY: unchecked((short)0x9ABC),
  WallType: 0xBCDE,
  AllowsRleBatching: false);
byte[] typedTileSectionFrame = TerrariaV1456Compatibility.EncodeTileSection(
  originX: 10,
  originY: 20,
  width: 1,
  height: 1,
  tiles: [sourceCompleteTile]);
TerrariaFrame typedTileSection = TerrariaFrameCodec.Decode(typedTileSectionFrame);
byte[] typedTileSectionPayload = DecompressTileSectionPayload(typedTileSection.Payload);
if (typedTileSection.MessageId != TerrariaMessageId.TileSection ||
    !typedTileSectionPayload.AsSpan().SequenceEqual(
      Convert.FromHexString("0A00000014000000010001002F4FFF1E34127856BC9AA0DEF011BC000000000000")))
{
  throw new InvalidOperationException(
    "V1456 TileSection typed tile record did not preserve the source byte order: " +
    Convert.ToHexString(typedTileSectionPayload));
}

byte[] typedTileEntitySectionFrame = TerrariaV1456Compatibility.EncodeTileSection(
  originX: 0,
  originY: 0,
  width: 1,
  height: 2,
  tiles: [default, default],
  tileEntities:
  [
    new LegacyItemTileEntity(
      LegacyTileEntityItemKind.ItemFrame,
      42,
      10,
      20,
      new LegacyTileEntityItem(500, 7, 3)),
    new LegacyTeleportationPylonTileEntity(43, 11, 21)
  ]);
TerrariaFrame typedTileEntitySection = TerrariaFrameCodec.Decode(typedTileEntitySectionFrame);
byte[] typedTileEntitySectionPayload = DecompressTileSectionPayload(
  typedTileEntitySection.Payload);
if (typedTileEntitySection.MessageId != TerrariaMessageId.TileSection ||
    !typedTileEntitySectionPayload.AsSpan().SequenceEqual(
      Convert.FromHexString("0000000000000000010002000000000000000200012A0000000A001400F401070300" +
        "072B0000000B001500")))
{
  throw new InvalidOperationException(
    "V1456 TileSection typed entities did not preserve the source byte order: " +
    Convert.ToHexString(typedTileEntitySectionPayload));
}

LegacyTileEntityItem?[] displayEquipment =
[
  new LegacyTileEntityItem(1000, 4, 5),
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  new LegacyTileEntityItem(301, 6, 7)
];
LegacyTileEntityItem?[] displayDyes =
[
  new LegacyTileEntityItem(210, 8, 9),
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  new LegacyTileEntityItem(303, 10, 11)
];
byte[] sparseTileEntitySectionFrame = TerrariaV1456Compatibility.EncodeTileSection(
  originX: 0,
  originY: 0,
  width: 1,
  height: 1,
  tiles: [default],
  tileEntities:
  [
    new LegacyHatRackTileEntity(
      1,
      2,
      3,
      new LegacyTileEntityItem(100, 1, 2),
      null,
      null,
      new LegacyTileEntityItem(200, 3, 4)),
    new LegacyDisplayDollTileEntity(
      4,
      5,
      6,
      displayEquipment,
      displayDyes,
      new LegacyTileEntityItem(400, 12, 13),
      7)
  ]);
TerrariaFrame sparseTileEntitySection = TerrariaFrameCodec.Decode(sparseTileEntitySectionFrame);
byte[] sparseTileEntitySectionPayload = DecompressTileSectionPayload(
  sparseTileEntitySection.Payload);
if (!sparseTileEntitySectionPayload.AsSpan().SequenceEqual(Convert.FromHexString(
      "00000000000000000100010000000000000200050100000002000300096400010200C800030400" +
      "03040000000500060001010707E8030405002D01060700D2000809002F010A0B0090010C0D00")))
{
  throw new InvalidOperationException(
    "V1456 TileSection sparse entity masks did not preserve source byte order: " +
    Convert.ToHexString(sparseTileEntitySectionPayload));
}

Console.WriteLine("PASS: V1456 compatibility length contracts");

foreach (byte messageId in new byte[] { 69 })
{
  TerrariaMessageDescriptor descriptor = TerrariaMessageCatalog.Get(
    (TerrariaMessageId)messageId);
  if (descriptor.Support != TerrariaPacketSupport.Unsupported)
  {
    throw new InvalidOperationException(
      $"Unsupported wiring message {messageId} was not isolated from mutation.");
  }
}

TerrariaMessageDescriptor liquidDescriptor = TerrariaMessageCatalog.Get(
  TerrariaMessageId.NetModules);
if (liquidDescriptor.Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException("NetModules liquid boundary is not cataloged.");
}

TerrariaMessageDescriptor tileEntitySharingDescriptor = TerrariaMessageCatalog.Get(
  TerrariaMessageId.TileEntitySharing);
TerrariaMessageDescriptor tileEntityPlacementDescriptor = TerrariaMessageCatalog.Get(
  TerrariaMessageId.TileEntityPlacement);
if (tileEntitySharingDescriptor.Direction != TerrariaPacketDirection.ServerToClient ||
    tileEntitySharingDescriptor.Support != TerrariaPacketSupport.Handled ||
    tileEntityPlacementDescriptor.Direction != TerrariaPacketDirection.Bidirectional ||
    tileEntityPlacementDescriptor.Support != TerrariaPacketSupport.Handled)
{
  throw new InvalidOperationException(
    "TrainingDummy tile-entity message directions are not source-faithful.");
}

TerrariaPacketDispatchResult placementResult = new TerrariaPacketDispatcher().Dispatch(
  new TerrariaSession(1),
  TerrariaPacketCodec.EncodeTrainingDummyTileEntityPlacement(12, 14));
if (placementResult.Outcome != TerrariaPacketDispatchOutcome.TileEntityPlacementAccepted ||
    placementResult.TileEntityPlacement != new TileEntityPlacementIntent(12, 14, 0))
{
  throw new InvalidOperationException(
    "Inbound TrainingDummy placement was not decoded as a typed intent.");
}

Console.WriteLine("PASS: unsupported wiring messages are explicitly isolated and NetModules is typed");
Console.WriteLine("PASS: TrainingDummy sharing is server-owned and placement is a typed bidirectional intent");

static TerrariaSession CreateActiveSession(byte assignedPlayerSlot)
{
  TerrariaSession session = new(assignedPlayerSlot);
  _ = session.AcceptHello(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaColor color = new(0, 0, 0);
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(new PlayerProfilePacket(
    assignedPlayerSlot,
    0,
    0,
    0.0f,
    0,
    "Compatibility",
    0,
    0,
    0,
    color,
    color,
    color,
    color,
    color,
    color,
    color,
    0,
    0,
    0)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    "7e8f76a0-c3bc-41f1-bdc1-50b75fe1e8d8")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    assignedPlayerSlot,
    2100,
    300,
    0,
    0,
    0,
    0,
    0)));
  return session;
}

static byte[] DecompressTileSectionPayload(ReadOnlyMemory<byte> payload)
{
  using MemoryStream compressedStream = new(payload.ToArray(), writable: false);
  using DeflateStream decompressor = new(
    compressedStream,
    CompressionMode.Decompress,
    leaveOpen: false);
  using MemoryStream uncompressedStream = new();
  decompressor.CopyTo(uncompressedStream);
  return uncompressedStream.ToArray();
}

static void AssertTruncatedPlayerControlsSuffix(byte[] frameBytes, string expectedMessage)
{
  try
  {
    _ = TerrariaPacketCodec.DecodePlayerControlsCompatibility(frameBytes);
  }
  catch (InvalidDataException exception) when (exception.Message.Contains(expectedMessage))
  {
    return;
  }

  throw new InvalidOperationException(
    $"V1456 PlayerControls did not reject its {expectedMessage} boundary.");
}

static byte[] CreatePlayerControlsFrame(
  byte playerSlot,
  byte flags1,
  byte flags2,
  byte flags3,
  byte flags4,
  byte selectedItem,
  float positionX,
  float positionY,
  SimulationVector? velocity = null,
  ushort? mountType = null,
  SimulationVector? returnOrigin = null,
  SimulationVector? returnHome = null,
  SimulationVector? cameraTarget = null,
  byte[]? trailingData = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write(playerSlot);
    writer.Write(flags1);
    writer.Write(flags2);
    writer.Write(flags3);
    writer.Write(flags4);
    writer.Write(selectedItem);
    writer.Write(positionX);
    writer.Write(positionY);
    if (velocity is SimulationVector velocityValue)
    {
      writer.Write(velocityValue.X);
      writer.Write(velocityValue.Y);
    }

    if (mountType is ushort mountValue)
    {
      writer.Write(mountValue);
    }

    if (returnOrigin is SimulationVector origin && returnHome is SimulationVector home)
    {
      writer.Write(origin.X);
      writer.Write(origin.Y);
      writer.Write(home.X);
      writer.Write(home.Y);
    }

    if (cameraTarget is SimulationVector camera)
    {
      writer.Write(camera.X);
      writer.Write(camera.Y);
    }

    if (trailingData is not null)
    {
      writer.Write(trailingData);
    }
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.PlayerControls,
    payload.ToArray()));
}

static byte[] CreateClientTextModuleFrame(
  string commandId,
  string text,
  byte[]? trailingData = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)1);
    writer.Write(commandId);
    writer.Write(text);
    if (trailingData is not null)
    {
      writer.Write(trailingData);
    }
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static byte[] CreateCreativePowerModuleFrame(
  ushort powerId,
  Action<BinaryWriter>? writePayload = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)5);
    writer.Write(powerId);
    writePayload?.Invoke(writer);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static byte[] CreateBannerModuleFrame(
  BannerModuleMessageType messageType,
  Action<BinaryWriter>? writePayload = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)10);
    writer.Write((byte)messageType);
    writePayload?.Invoke(writer);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static byte[] CreateCraftingModuleFrame(Action<BinaryWriter> writePayload)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)11);
    writePayload(writer);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static byte[] CreateTagEffectModuleFrame(
  byte ownerSlot,
  TagEffectMessageType messageType,
  Action<BinaryWriter>? writePayload = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)12);
    writer.Write(ownerSlot);
    writer.Write((byte)messageType);
    writePayload?.Invoke(writer);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static byte[] CreateLeashedEntityModuleFrame(
  LeashedEntityMessageType messageType,
  int slot,
  Action<BinaryWriter>? writePayload = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)13);
    writer.Write((byte)messageType);
    writer.Write7BitEncodedInt(slot);
    writePayload?.Invoke(writer);
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static void WriteLeashedCritterState(
  BinaryWriter writer,
  bool full,
  byte? extensionValue)
{
  if (full)
  {
    writer.Write7BitEncodedInt(55);
    writer.Write(16.0f);
    writer.Write(24.0f);
  }

  writer.Write(0xAABBCCDDu);
  writer.Write(true);
  writer.Write(0x01020304u);
  writer.Write((short)-50);
  writer.Write((byte)6);
  writer.Write((sbyte)-2);
  writer.Write((sbyte)3);
  if (extensionValue is byte value)
  {
    writer.Write(value);
  }
}

static byte[] CreateServerTextModuleFrame(
  byte authorId,
  LegacyNetworkText? text,
  TerrariaColor color,
  byte? modeByte = null,
  byte[]? trailingData = null)
{
  using MemoryStream payload = new();
  using (BinaryWriter writer = new(payload))
  {
    writer.Write((ushort)1);
    writer.Write(authorId);
    if (modeByte is byte rawMode)
    {
      writer.Write(rawMode);
      writer.Write(string.Empty);
    }
    else
    {
      WriteNetworkTextForTest(writer, text!);
    }

    writer.Write(color.Red);
    writer.Write(color.Green);
    writer.Write(color.Blue);
    if (trailingData is not null)
    {
      writer.Write(trailingData);
    }
  }

  return TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.NetModules,
    payload.ToArray()));
}

static void WriteNetworkTextForTest(BinaryWriter writer, LegacyNetworkText text)
{
  writer.Write((byte)text.Mode);
  writer.Write(text.Text);
  if (text.Mode == LegacyNetworkTextMode.Literal)
  {
    return;
  }

  writer.Write((byte)text.Substitutions.Count);
  for (int index = 0; index < text.Substitutions.Count; index++)
  {
    WriteNetworkTextForTest(writer, text.Substitutions[index]);
  }
}

static void VerifyPlayerControlsProjection(
  string name,
  byte[] frameBytes,
  int expectedPayloadLength,
  SimulationVector? expectedVelocity,
  ushort? expectedMountType,
  SimulationVector? expectedReturnOrigin,
  SimulationVector? expectedReturnHome,
  SimulationVector? expectedCameraTarget)
{
  TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
  LegacyPlayerControlsProjection projection =
    TerrariaPacketCodec.DecodePlayerControlsCompatibility(frameBytes);
  if (frame.Payload.Length != expectedPayloadLength ||
      projection.State.Velocity != expectedVelocity ||
      projection.State.MountType != expectedMountType ||
      projection.State.ReturnOrigin != expectedReturnOrigin ||
      projection.State.ReturnHome != expectedReturnHome ||
      projection.State.CameraTarget != expectedCameraTarget ||
      projection.Intent.PlayerSlot != projection.State.PlayerSlot ||
      projection.Intent.SelectedItem != projection.State.SelectedItem)
  {
    throw new InvalidOperationException(
      $"V1456 PlayerControls {name} suffix projection did not match its wire form.");
  }
}
