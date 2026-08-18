using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Simulation.Players;

namespace Terraria.Dome.Server.Protocol;

internal static class PlayerPersistentStateMapper
{
  internal const int ConnectionEquipmentSlotCount = 189;

  private const int DefaultLife = 100;
  private const int DefaultMana = 20;
  private const int FirstLoadoutConnectionSlot = 900;
  private const int LastBaseConnectionSlot = 98;
  private const int LastLoadoutConnectionSlot = 989;

  public static PlayerPersistentState FromBootstrap(PlayerBootstrapState bootstrap)
  {
    ArgumentNullException.ThrowIfNull(bootstrap);
    PlayerVitalsPacket life = bootstrap.Life ?? new PlayerVitalsPacket(0, DefaultLife, DefaultLife);
    PlayerVitalsPacket mana = bootstrap.Mana ?? new PlayerVitalsPacket(0, DefaultMana, DefaultMana);
    PlayerLoadoutPacket loadout = bootstrap.Loadout ?? new PlayerLoadoutPacket(
      0,
      0,
      bootstrap.Profile.AccessoryVisibility);
    List<PlayerPersistentBuff> buffs = new(bootstrap.BuffTypes.Count);
    for (int index = 0; index < bootstrap.BuffTypes.Count; index++)
    {
      buffs.Add(new PlayerPersistentBuff(bootstrap.BuffTypes[index]));
    }

    List<PlayerPersistentItem> items = new(bootstrap.Equipment.Count);
    for (int index = 0; index < bootstrap.Equipment.Count; index++)
    {
      PlayerEquipmentPacket item = bootstrap.Equipment[index];
      items.Add(new PlayerPersistentItem(
        item.SlotId,
        item.Stack,
        item.Prefix,
        item.ItemType,
        item.IsFavorited,
        item.IsNewAndShiny));
    }

    return new PlayerPersistentState(
      bootstrap.Uuid,
      ToPersistentProfile(bootstrap.Profile),
      life.Current,
      life.Maximum,
      mana.Current,
      mana.Maximum,
      buffs,
      loadout.SelectedLoadout,
      loadout.AccessoryVisibility,
      items);
  }

  public static IReadOnlyList<byte[]> ToAuthorityFrames(
    byte playerSlot,
    PlayerPersistentState account)
  {
    ArgumentNullException.ThrowIfNull(account);
    List<byte[]> frames = new(ConnectionEquipmentSlotCount + 4)
    {
      TerrariaPacketCodec.EncodePlayerLifeMana(playerSlot, account.Life, account.MaximumLife),
      TerrariaPacketCodec.EncodePlayerMana(playerSlot, account.Mana, account.MaximumMana),
      TerrariaPacketCodec.Encode(new PlayerBuffsPacket(
        playerSlot,
        ToBuffTypes(account.Buffs))),
      TerrariaPacketCodec.Encode(new PlayerLoadoutPacket(
        playerSlot,
        account.SelectedLoadout,
        account.AccessoryVisibility))
    };
    for (int slotId = 0; slotId < account.Items.Count; slotId++)
    {
      PlayerPersistentItem item = account.Items[slotId];
      if (!IsConnectionVisibleItemSlot(item.SlotId))
      {
        continue;
      }

      frames.Add(TerrariaPacketCodec.Encode(new PlayerEquipmentPacket(
        playerSlot,
        item.SlotId,
        item.Stack,
        item.Prefix,
        item.ItemType,
        item.IsFavorited,
        item.IsNewAndShiny)));
    }

    return frames;
  }

  public static byte[] ToProfileFrame(byte playerSlot, PlayerPersistentState account)
  {
    ArgumentNullException.ThrowIfNull(account);
    return TerrariaPacketCodec.Encode(ToPlayerProfilePacket(playerSlot, account.Profile));
  }

  private static IReadOnlyList<ushort> ToBuffTypes(IReadOnlyList<PlayerPersistentBuff> buffs)
  {
    ushort[] buffTypes = new ushort[buffs.Count];
    for (int index = 0; index < buffTypes.Length; index++)
    {
      buffTypes[index] = buffs[index].Type;
    }

    return buffTypes;
  }

  private static bool IsConnectionVisibleItemSlot(int slotId)
  {
    return slotId >= 0 &&
      (slotId <= LastBaseConnectionSlot ||
       slotId >= FirstLoadoutConnectionSlot && slotId <= LastLoadoutConnectionSlot);
  }

  private static PlayerPersistentColor ToPersistentColor(TerrariaColor color)
  {
    return new PlayerPersistentColor(color.Red, color.Green, color.Blue);
  }

  private static PlayerPersistentProfile ToPersistentProfile(PlayerProfilePacket profile)
  {
    return new PlayerPersistentProfile(
      profile.Name,
      profile.SkinVariant,
      profile.VoiceVariant,
      profile.VoicePitchOffset,
      profile.Hair,
      profile.HairDye,
      profile.AccessoryVisibility,
      profile.HideMisc,
      ToPersistentColor(profile.HairColor),
      ToPersistentColor(profile.SkinColor),
      ToPersistentColor(profile.EyeColor),
      ToPersistentColor(profile.ShirtColor),
      ToPersistentColor(profile.UnderShirtColor),
      ToPersistentColor(profile.PantsColor),
      ToPersistentColor(profile.ShoeColor),
      profile.DifficultyFlags,
      profile.BiomeTorchFlags,
      profile.ConsumableFlags);
  }

  private static TerrariaColor ToTerrariaColor(PlayerPersistentColor color)
  {
    return new TerrariaColor(color.Red, color.Green, color.Blue);
  }

  private static PlayerProfilePacket ToPlayerProfilePacket(
    byte playerSlot,
    PlayerPersistentProfile profile)
  {
    return new PlayerProfilePacket(
      playerSlot,
      profile.SkinVariant,
      profile.VoiceVariant,
      profile.VoicePitchOffset,
      profile.Hair,
      profile.Name,
      profile.HairDye,
      profile.AccessoryVisibility,
      profile.HideMisc,
      ToTerrariaColor(profile.HairColor),
      ToTerrariaColor(profile.SkinColor),
      ToTerrariaColor(profile.EyeColor),
      ToTerrariaColor(profile.ShirtColor),
      ToTerrariaColor(profile.UnderShirtColor),
      ToTerrariaColor(profile.PantsColor),
      ToTerrariaColor(profile.ShoeColor),
      profile.DifficultyFlags,
      profile.BiomeTorchFlags,
      profile.ConsumableFlags);
  }
}
