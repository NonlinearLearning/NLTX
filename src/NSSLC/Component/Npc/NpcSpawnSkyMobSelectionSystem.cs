using System;

namespace Terraria.Npc;

public static class NpcSpawnSkyMobSelectionSystem
{
  private const int PixelsPerTile = 16;
  private const int SpawnTilePixelCenterOffset = 8;
  private const int DefaultStartIndex = 0;
  private const int DefaultTarget = 255;
  private const int InvasionTypeFourSkyNpcType = 388;
  private const int MartianSaucerType = 399;
  private const int WyvernType = 87;
  private const int PurpleSlimeType = 686;
  private const int DefaultSkyNpcType = 48;

  public static NpcSpawnSkyMobSelectionResult Select(
    in NpcSpawnSkyMobSelectionInputs inputs,
    INpcSpawnSkyMobSelectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    if (!inputs.IsSkyMob)
    {
      return new NpcSpawnSkyMobSelectionResult(
        NpcSpawnSkyMobSelectionStatus.NotApplicable,
        null);
    }

    int maxValue2 = inputs.ZoneWaterCandle ? 3 : 8;
    int maxValue3 = inputs.ZoneWaterCandle ? 10 : 30;
    bool flag5 = (float)Math.Abs(inputs.SpawnTileX - inputs.MaxTilesX / 2) /
      (float)(inputs.MaxTilesX / 2) > 0.33f && inputs.SkyBehindPlayer;
    if (flag5 && port.AnyDanger())
    {
      flag5 = false;
    }

    int selectedType;
    if (inputs.Invaders && inputs.InvasionType == 4)
    {
      selectedType = InvasionTypeFourSkyNpcType;
    }
    else if (flag5 && inputs.HardMode && inputs.DownedGolemBoss &&
      ((!inputs.DownedMartians && Next(port, maxValue2) == 0) ||
        Next(port, maxValue3) == 0) && !port.HasActiveNpc(MartianSaucerType))
    {
      selectedType = MartianSaucerType;
    }
    // The legacy duplicate retries the random condition before checking the candle flag.
    else if (flag5 && inputs.HardMode && inputs.DownedGolemBoss &&
      ((!inputs.DownedMartians && Next(port, maxValue2) == 0) ||
        Next(port, maxValue3) == 0) && !port.HasActiveNpc(MartianSaucerType) &&
      inputs.ZoneWaterCandle)
    {
      selectedType = MartianSaucerType;
    }
    else if (inputs.HardMode && !port.HasActiveNpc(WyvernType) &&
      !inputs.NoWorms && Next(port, 10) == 0)
    {
      selectedType = WyvernType;
    }
    // The legacy duplicate can consume a second draw after the first roll fails.
    else if (inputs.HardMode && !port.HasActiveNpc(WyvernType) &&
      !inputs.NoWorms && Next(port, 10) == 0 && inputs.ZoneWaterCandle)
    {
      selectedType = WyvernType;
    }
    else if (!inputs.UnlockedSlimePurpleSpawn && port.RollLuck(25) == 0 &&
      !port.HasActiveNpc(PurpleSlimeType))
    {
      selectedType = PurpleSlimeType;
    }
    else
    {
      selectedType = DefaultSkyNpcType;
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
    return new NpcSpawnSkyMobSelectionResult(
      NpcSpawnSkyMobSelectionStatus.RequestProduced,
      request);
  }

  private static int Next(INpcSpawnSkyMobSelectionPort port, int exclusiveUpperBound)
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
