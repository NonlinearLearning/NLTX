using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public sealed record PlayerAppearance(
  byte SkinVariant,
  byte VoiceVariant,
  float VoicePitchOffset,
  byte Hair,
  string Name,
  byte HairDye,
  ushort HiddenAccessories,
  byte HideMisc,
  PacketRgb HairColor,
  PacketRgb SkinColor,
  PacketRgb EyeColor,
  PacketRgb ShirtColor,
  PacketRgb UnderShirtColor,
  PacketRgb PantsColor,
  PacketRgb ShoeColor,
  byte DifficultyAndAccessoryFlags,
  byte BiomeAndCartFlags,
  byte PermanentUpgradeFlags)
{
  public static PlayerAppearance Initial => new(
    0,
    1,
    0,
    0,
    string.Empty,
    0,
    0,
    0,
    default,
    default,
    default,
    default,
    default,
    default,
    default,
    0,
    0,
    0);

  public bool IsJourneyCharacter => (DifficultyAndAccessoryFlags & 0b0000_1000) != 0;

  public bool HasExtraAccessorySlot => (DifficultyAndAccessoryFlags & 0b0000_0100) != 0;

  public bool HasUsedGalaxyPearl => (PermanentUpgradeFlags & 0b0000_1000) != 0;
}
