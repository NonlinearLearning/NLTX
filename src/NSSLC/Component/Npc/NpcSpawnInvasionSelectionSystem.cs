using System;

namespace Terraria.Npc;

public static class NpcSpawnInvasionSelectionSystem
{
  private const int PixelsPerTile = 16;
  private const int SpawnTilePixelCenterOffset = 8;
  private const int MartianSaucerSpawnOffsetTiles = 10;
  private const int DefaultStartIndex = 0;
  private const int PirateInvasionStartIndex = 1;
  private const int DefaultTarget = 255;

  private const int GoblinSummonerType = 471;
  private const int GoblinWarriorType = 29;
  private const int GoblinArcherType = 26;
  private const int GoblinSorcererType = 111;
  private const int GoblinPeonType = 27;
  private const int GoblinThiefType = 28;

  private const int FrostQueenType = 145;
  private const int ZombieElfType = 143;
  private const int GingerbreadManType = 144;

  private const int MartianSaucerType = 491;
  private const int GrayGruntType = 216;
  private const int BrainScramblerType = 215;
  private const int RayGunnerType = 252;
  private const int MartianEngineerType = 214;
  private const int MartianOfficerType = 213;
  private const int MartianProbeType = 212;

  private const int PirateCaptainType = 395;
  private const int PirateCorsairType = 390;
  private const int PirateDeadeyeType = 386;
  private const int PirateCrossbowerType = 382;
  private const int PirateDeadeyeAlternativeType = 388;
  private const int PirateCorsairAlternativeType = 381;
  private const int PirateDeckhandType = 385;
  private const int PirateParrotType = 389;
  private const int PirateDeadeyeFallbackType = 383;
  private const int PirateParrotPresenceGateType = 520;

  public static NpcSpawnInvasionSelectionResult Select(
    in NpcSpawnInvasionSelectionInputs inputs,
    INpcSpawnInvasionSelectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    if (!inputs.Invaders)
    {
      return new NpcSpawnInvasionSelectionResult(
        NpcSpawnInvasionSelectionStatus.NotApplicable,
        null);
    }

    int selectedType;
    int startIndex = DefaultStartIndex;
    switch (inputs.InvasionType)
    {
      case 1:
        selectedType = SelectGoblinInvasionType(inputs.HardMode, port);
        break;
      case 2:
        selectedType = SelectFrostInvasionType(port);
        break;
      case 3:
        selectedType = SelectMartianInvasionType(inputs, port);
        break;
      case 4:
        selectedType = SelectPirateInvasionType(inputs, port);
        startIndex = PirateInvasionStartIndex;
        break;
      default:
        return new NpcSpawnInvasionSelectionResult(
          NpcSpawnInvasionSelectionStatus.UnknownInvasionTypeEarlyReturn,
          null);
    }

    if (selectedType == 0)
    {
      return new NpcSpawnInvasionSelectionResult(
        NpcSpawnInvasionSelectionStatus.HandledWithoutRequest,
        null);
    }

    NpcSpawnEntityRequest request = new(
      Type: new NpcTypeId(selectedType),
      PositionX: inputs.SpawnTileX * PixelsPerTile + SpawnTilePixelCenterOffset,
      PositionY: (inputs.SpawnTileY -
        (selectedType == MartianSaucerType ? MartianSaucerSpawnOffsetTiles : 0)) * PixelsPerTile,
      StartIndex: startIndex,
      Ai0: 0f,
      Ai1: 0f,
      Ai2: 0f,
      Ai3: 0f,
      Target: DefaultTarget);
    return new NpcSpawnInvasionSelectionResult(
      NpcSpawnInvasionSelectionStatus.RequestProduced,
      request);
  }

  private static int SelectGoblinInvasionType(
    bool hardMode,
    INpcSpawnInvasionSelectionPort port)
  {
    if (hardMode && !port.HasActiveNpc(GoblinSummonerType) && Next(port, 30) == 0)
    {
      return GoblinSummonerType;
    }

    if (Next(port, 9) == 0)
    {
      return GoblinWarriorType;
    }

    if (Next(port, 5) == 0)
    {
      return GoblinArcherType;
    }

    if (Next(port, 3) == 0)
    {
      return GoblinSorcererType;
    }

    if (Next(port, 3) == 0)
    {
      return GoblinPeonType;
    }

    return GoblinThiefType;
  }

  private static int SelectFrostInvasionType(INpcSpawnInvasionSelectionPort port)
  {
    if (Next(port, 7) == 0)
    {
      return FrostQueenType;
    }

    if (Next(port, 3) == 0)
    {
      return ZombieElfType;
    }

    return GingerbreadManType;
  }

  private static int SelectMartianInvasionType(
    in NpcSpawnInvasionSelectionInputs inputs,
    INpcSpawnInvasionSelectionPort port)
  {
    if (inputs.InvasionSize < inputs.InvasionSizeStart / 2 &&
      Next(port, 20) == 0 &&
      !port.HasActiveNpc(MartianSaucerType) &&
      !port.HasAnySolidTiles(
        inputs.SpawnTileX - 20,
        inputs.SpawnTileX + 20,
        inputs.SpawnTileY - 40,
        inputs.SpawnTileY - 10))
    {
      return MartianSaucerType;
    }

    if (Next(port, 30) == 0 && !port.HasActiveNpc(GrayGruntType))
    {
      return GrayGruntType;
    }

    if (Next(port, 11) == 0)
    {
      return BrainScramblerType;
    }

    if (Next(port, 9) == 0)
    {
      return RayGunnerType;
    }

    if (Next(port, 7) == 0)
    {
      return MartianEngineerType;
    }

    if (Next(port, 3) == 0)
    {
      return MartianOfficerType;
    }

    return MartianProbeType;
  }

  private static int SelectPirateInvasionType(
    in NpcSpawnInvasionSelectionInputs inputs,
    INpcSpawnInvasionSelectionPort port)
  {
    int pirateRoll = Next(port, 7);
    bool canSpawnCaptain =
      (float)(inputs.InvasionSizeStart - inputs.InvasionSize) /
        (float)inputs.InvasionSizeStart >= 0.3f &&
      !port.HasActiveNpc(PirateCaptainType);

    if (Next(port, 45) == 0 && canSpawnCaptain)
    {
      return PirateCaptainType;
    }

    if (pirateRoll >= 6)
    {
      if (Next(port, 20) == 0 && canSpawnCaptain)
      {
        return PirateCaptainType;
      }

      return Next(port, 2) == 0 ? PirateCorsairType : PirateDeadeyeType;
    }

    if (pirateRoll >= 4)
    {
      int pirateChoice = Next(port, 5);
      return pirateChoice < 2
        ? PirateCrossbowerType
        : pirateChoice >= 4
          ? PirateDeadeyeAlternativeType
          : PirateCorsairAlternativeType;
    }

    int pirateTypeChoice = Next(port, 4);
    if (pirateTypeChoice == 3)
    {
      if (!port.HasActiveNpc(PirateParrotPresenceGateType))
      {
        return PirateParrotPresenceGateType;
      }

      pirateTypeChoice = Next(port, 3);
    }

    return pirateTypeChoice switch
    {
      0 => PirateDeckhandType,
      1 => PirateParrotType,
      2 => PirateDeadeyeFallbackType,
      _ => 0,
    };
  }

  private static int Next(INpcSpawnInvasionSelectionPort port, int exclusiveUpperBound)
  {
    int value = port.Next(exclusiveUpperBound);
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The spawn random port returned a value outside its requested range.");
    }

    return value;
  }
}
