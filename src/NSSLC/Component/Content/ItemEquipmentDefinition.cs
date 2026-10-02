namespace Terraria.Content;

public sealed record ItemEquipmentDefinition(
  bool IsAccessory,
  int? HeadSlot,
  int? BodySlot,
  int? LegSlot,
  sbyte? HandOnSlot = null,
  sbyte? HandOffSlot = null,
  sbyte? BackSlot = null,
  sbyte? FrontSlot = null,
  sbyte? ShoeSlot = null,
  sbyte? WaistSlot = null,
  sbyte? WingSlot = null,
  sbyte? ShieldSlot = null,
  sbyte? NeckSlot = null,
  sbyte? FaceSlot = null,
  sbyte? BalloonSlot = null,
  sbyte? BeardSlot = null,
  sbyte? VoiceSlot = null,
  int Defense = 0,
  bool IsSocial = false,
  bool IsVanity = false,
  bool HasVanityEffects = false);
