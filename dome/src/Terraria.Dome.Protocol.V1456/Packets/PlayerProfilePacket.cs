namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct PlayerProfilePacket(
  byte PlayerSlot,
  byte SkinVariant,
  byte VoiceVariant,
  float VoicePitchOffset,
  byte Hair,
  string Name,
  byte HairDye,
  ushort AccessoryVisibility,
  byte HideMisc,
  TerrariaColor HairColor,
  TerrariaColor SkinColor,
  TerrariaColor EyeColor,
  TerrariaColor ShirtColor,
  TerrariaColor UnderShirtColor,
  TerrariaColor PantsColor,
  TerrariaColor ShoeColor,
  byte DifficultyFlags,
  byte BiomeTorchFlags,
  byte ConsumableFlags);
