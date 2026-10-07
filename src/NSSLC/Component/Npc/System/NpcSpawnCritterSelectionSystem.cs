using System;

namespace Terraria.Npc;

public static class NpcSpawnCritterSelectionSystem
{
  private const int PixelsPerTile = 16;
  private const int SpawnTilePixelCenterOffset = 8;
  private const int DefaultStartIndex = 0;
  private const int DefaultTarget = 255;
  private const int NormalTimeLeftMultiplier = 1;
  private const int GnomeTimeLeftMultiplier = 10;

  private const int GoldfishType = 592;
  private const int GoldCritterType = 55;
  private const int UndergroundGoldCritterType = 447;
  private const int UndergroundCritterType = 300;
  private const int UndergroundRareCritterType = 359;
  private const int UndergroundGemCritterType = 448;
  private const int UndergroundSurfaceCritterType = 357;
  private const int GnomeType = 624;
  private const int GoldBunnyType = 443;
  private const int GoldSquirrelType = 539;
  private const int HalloweenCritterType = 303;
  private const int ChristmasCritterType = 337;
  private const int PartyCritterType = 540;
  private const int CommonSurfaceCritterType = 46;
  private const int GemSquirrelType = 299;
  private const int GemRabbitType = 538;

  public static NpcSpawnCritterSelectionResult Select(
    in NpcSpawnCritterSelectionInputs inputs,
    INpcSpawnCritterSelectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    if (inputs.RemixWorld)
    {
      return CreateResult(NpcSpawnCritterSelectionStatus.NotApplicable);
    }

    int selectedType;
    int timeLeftMultiplier = NormalTimeLeftMultiplier;
    if (inputs.WaterTile)
    {
      selectedType = port.RollLuck(inputs.GoldCritterChance) == 0
        ? GoldfishType
        : GoldCritterType;
    }
    else if ((double)inputs.SpawnTileY > inputs.WorldSurface)
    {
      if (Next(port, 3) == 0)
      {
        selectedType = port.RollLuck(inputs.GoldCritterChance) == 0
          ? UndergroundGoldCritterType
          : UndergroundCritterType;
      }
      else if (Next(port, 2) == 0)
      {
        selectedType = UndergroundRareCritterType;
      }
      else if (port.RollLuck(inputs.GoldCritterChance) == 0)
      {
        selectedType = UndergroundGemCritterType;
      }
      else if (Next(port, 3) != 0)
      {
        selectedType = UndergroundSurfaceCritterType;
      }
      else
      {
        return CreateResult(NpcSpawnCritterSelectionStatus.HandledWithoutRequest);
      }
    }
    else if (port.RollLuck(1 + inputs.GnomeChance / 10) == 0)
    {
      selectedType = GnomeType;
      timeLeftMultiplier = GnomeTimeLeftMultiplier;
    }
    else if (port.RollLuck(inputs.GoldCritterChance) == 0)
    {
      selectedType = GoldBunnyType;
    }
    else if (port.RollLuck(inputs.GoldCritterChance) == 0)
    {
      selectedType = GoldSquirrelType;
    }
    else if (inputs.Halloween && Next(port, 3) != 0)
    {
      selectedType = HalloweenCritterType;
    }
    else if (inputs.Christmas && Next(port, 3) != 0)
    {
      selectedType = ChristmasCritterType;
    }
    else if (inputs.BirthdayPartyActive && Next(port, 3) != 0)
    {
      selectedType = PartyCritterType;
    }
    else if (Next(port, 3) == 0)
    {
      // Utils.SelectRandom uses one Next(choices.Length) draw in this branch.
      selectedType = Next(port, 2) == 0 ? GemSquirrelType : GemRabbitType;
    }
    else
    {
      selectedType = CommonSurfaceCritterType;
    }

    NpcSpawnEntityRequest request = new(
      Type: new NpcTypeId(selectedType),
      PositionX: inputs.SpawnTileX * PixelsPerTile + SpawnTilePixelCenterOffset,
      PositionY: inputs.SpawnTileY * PixelsPerTile,
      StartIndex: DefaultStartIndex,
      Ai0: 0f,
      Ai1: 0f,
      Ai2: 0f,
      Ai3: 0f,
      Target: DefaultTarget);
    return new NpcSpawnCritterSelectionResult(
      NpcSpawnCritterSelectionStatus.RequestProduced,
      request,
      timeLeftMultiplier);
  }

  private static NpcSpawnCritterSelectionResult CreateResult(
    NpcSpawnCritterSelectionStatus status)
  {
    return new NpcSpawnCritterSelectionResult(
      status,
      null,
      NormalTimeLeftMultiplier);
  }

  private static int Next(INpcSpawnCritterSelectionPort port, int exclusiveUpperBound)
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
