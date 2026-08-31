namespace Terraria.Dome.Simulation.Items.Definitions;

public enum ItemEquipmentSlot : byte
{
  None,
  Head,
  Body,
  Legs,
  Accessory,
  Wings,
  Shield,
  Vanity
}

public readonly record struct ItemEquipmentDefinition(
  ItemEquipmentSlot Slot = ItemEquipmentSlot.None,
  int Defense = 0,
  int LifeRegen = 0,
  int ManaIncrease = 0,
  int HeadSlot = -1,
  int BodySlot = -1,
  int LegSlot = -1,
  sbyte HandOnSlot = -1,
  sbyte HandOffSlot = -1,
  sbyte BackSlot = -1,
  sbyte FrontSlot = -1,
  sbyte ShoeSlot = -1,
  sbyte WaistSlot = -1,
  sbyte WingSlot = -1,
  sbyte ShieldSlot = -1,
  sbyte NeckSlot = -1,
  sbyte FaceSlot = -1,
  sbyte BalloonSlot = -1,
  sbyte BeardSlot = -1,
  sbyte VoiceSlot = 0,
  bool HasVanityEffects = false,
  bool Accessory = false,
  bool Vanity = false,
  bool Social = false,
  int SentryCapacityBonus = 0);
