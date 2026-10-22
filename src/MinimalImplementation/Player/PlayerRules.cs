using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public static class PlayerRules
{
  public const int DefaultNameLengthLimit = 20;
  public const int MinimumVoiceVariant = 1;
  public const int MaximumVoiceVariant = 4;
  public const int InvalidHairThreshold = 228;

  public static PlayerSyncPreparation PrepareAppearance(
    SyncPlayerPacket packet,
    int skinVariantCount,
    int maximumNameLength,
    bool isJourneyWorld,
    IReadOnlyCollection<string> activePlayerNames,
    bool checkDuplicateName)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(activePlayerNames);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(skinVariantCount);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumNameLength);

    string name = packet.Name.Trim();
    if (checkDuplicateName && activePlayerNames.Any(existing =>
      string.Equals(existing, name, StringComparison.Ordinal)))
    {
      return Reject(ProtocolErrorCode.Conflict, "A player with this name is already connected.");
    }

    if (name.Length > maximumNameLength)
    {
      return Reject(ProtocolErrorCode.InvalidLength, "The player name exceeds the Version4 limit.");
    }

    if (name.Length == 0)
    {
      return Reject(ProtocolErrorCode.Forbidden, "The player name cannot be empty.");
    }

    byte difficulty = GetDifficulty(packet.DifficultyAndAccessoryFlags);
    if (difficulty == 3 && !isJourneyWorld)
    {
      return Reject(
        ProtocolErrorCode.Forbidden,
        "A Journey character cannot join a non-Journey world.");
    }

    if (difficulty != 3 && isJourneyWorld)
    {
      return Reject(
        ProtocolErrorCode.Forbidden,
        "Only a Journey character can join a Journey world.");
    }

    float pitch = float.IsFinite(packet.VoicePitchOffset)
      ? Math.Clamp(packet.VoicePitchOffset, -1f, 1f)
      : 0f;
    var appearance = new PlayerAppearance(
      (byte)Math.Clamp((int)packet.SkinVariant, 0, skinVariantCount - 1),
      (byte)Math.Clamp((int)packet.VoiceVariant, MinimumVoiceVariant, MaximumVoiceVariant),
      pitch,
      packet.Hair >= InvalidHairThreshold ? (byte)0 : packet.Hair,
      name,
      packet.HairDye,
      packet.HiddenAccessories,
      packet.HideMisc,
      packet.HairColor,
      packet.SkinColor,
      packet.EyeColor,
      packet.ShirtColor,
      packet.UnderShirtColor,
      packet.PantsColor,
      packet.ShoeColor,
      packet.DifficultyAndAccessoryFlags,
      packet.BiomeAndCartFlags,
      packet.PermanentUpgradeFlags);

    return new PlayerSyncPreparation(
      true,
      appearance,
      null,
      null);
  }

  public static byte GetDifficulty(byte difficultyAndAccessoryFlags)
  {
    byte difficulty = 0;
    if ((difficultyAndAccessoryFlags & 0b0000_0001) != 0)
    {
      difficulty = 1;
    }

    if ((difficultyAndAccessoryFlags & 0b0000_0010) != 0)
    {
      difficulty = 2;
    }

    if ((difficultyAndAccessoryFlags & 0b0000_1000) != 0)
    {
      difficulty = 3;
    }

    return difficulty;
  }

  private static PlayerSyncPreparation Reject(ProtocolErrorCode code, string message)
  {
    return new PlayerSyncPreparation(
      false,
      null,
      code,
      message);
  }
}
