using System;

namespace Terraria.Npc;

public static class NpcSpawnTowerSelectionSystem
{
  private const int PixelsPerTile = 16;
  private const int SpawnTilePixelCenterOffset = 8;
  private const int ReplacementSearchStartIndex = 1;
  private const int DefaultTarget = 255;

  private static readonly int[] NebulaChoices =
  [
    424, 424, 424, 423, 423, 423, 421, 421, 421, 420, 420,
  ];

  private static readonly int[] VortexChoices =
  [
    429, 429, 429, 429, 427, 427, 425, 425, 426,
  ];

  private static readonly int[] StardustChoices =
  [
    411, 411, 411, 409, 409, 407, 402, 405,
  ];

  private static readonly int[] SolarChoices =
  [
    518, 419, 418, 412, 417, 416, 415,
  ];

  private static readonly int[] SolarLunarReplacementChoices =
  [
    415, 416, 419, 417,
  ];

  public static NpcSpawnTowerSelectionResult Select(
    in NpcSpawnTowerSelectionInputs inputs,
    INpcSpawnTowerSelectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    int selectedType;
    if (inputs.EventAndTower.ZoneTowerNebula)
    {
      selectedType = SelectNebulaType(port);
    }
    else if (inputs.EventAndTower.ZoneTowerVortex)
    {
      selectedType = SelectVortexType(port);
    }
    else if (inputs.EventAndTower.ZoneTowerStardust)
    {
      selectedType = SelectWeightedType(StardustChoices, port);
    }
    else if (inputs.EventAndTower.ZoneTowerSolar)
    {
      selectedType = SelectSolarType(port);
    }
    else
    {
      return new NpcSpawnTowerSelectionResult(
        NpcSpawnTowerSelectionStatus.NotApplicable,
        null);
    }

    NpcSpawnEntityRequest request = new(
      Type: new NpcTypeId(selectedType),
      PositionX: inputs.SpawnTileX * PixelsPerTile + SpawnTilePixelCenterOffset,
      PositionY: inputs.SpawnTileY * PixelsPerTile,
      StartIndex: ReplacementSearchStartIndex,
      Ai0: 0f,
      Ai1: 0f,
      Ai2: 0f,
      Ai3: 0f,
      Target: DefaultTarget);
    return new NpcSpawnTowerSelectionResult(
      NpcSpawnTowerSelectionStatus.RequestProduced,
      request);
  }

  private static int SelectNebulaType(INpcSpawnTowerSelectionPort port)
  {
    while (true)
    {
      int npcTypeId = SelectWeightedType(NebulaChoices, port);
      if (npcTypeId == 424 && IsAtPopulationLimit(port, npcTypeId, 3))
      {
        continue;
      }

      if (npcTypeId == 423 && IsAtPopulationLimit(port, npcTypeId, 3))
      {
        continue;
      }

      if (npcTypeId == 420 && IsAtPopulationLimit(port, npcTypeId, 3))
      {
        continue;
      }

      return npcTypeId;
    }
  }

  private static int SelectVortexType(INpcSpawnTowerSelectionPort port)
  {
    while (true)
    {
      int npcTypeId = SelectWeightedType(VortexChoices, port);
      if (npcTypeId == 425 && IsAtPopulationLimit(port, npcTypeId, 3))
      {
        continue;
      }

      if (npcTypeId == 426 && IsAtPopulationLimit(port, npcTypeId, 3))
      {
        continue;
      }

      if (npcTypeId == 429 && IsAtPopulationLimit(port, npcTypeId, 4))
      {
        continue;
      }

      return npcTypeId;
    }
  }

  private static int SelectSolarType(INpcSpawnTowerSelectionPort port)
  {
    while (true)
    {
      int npcTypeId = SelectWeightedType(SolarChoices, port);
      if (npcTypeId == 418 && Next(port, 2) == 0)
      {
        npcTypeId = SelectWeightedType(SolarLunarReplacementChoices, port);
      }

      if (npcTypeId == 518 && IsAtPopulationLimit(port, npcTypeId, 2))
      {
        continue;
      }

      if (npcTypeId == 412 && IsAtPopulationLimit(port, npcTypeId, 1))
      {
        continue;
      }

      return npcTypeId;
    }
  }

  private static int SelectWeightedType(
    int[] choices,
    INpcSpawnTowerSelectionPort port)
  {
    return choices[Next(port, choices.Length)];
  }

  private static int Next(INpcSpawnTowerSelectionPort port, int exclusiveUpperBound)
  {
    int value = port.Next(exclusiveUpperBound);
    if (value < 0 || value >= exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The spawn random port returned a value outside its requested range.");
    }

    return value;
  }

  private static bool IsAtPopulationLimit(
    INpcSpawnTowerSelectionPort port,
    int npcTypeId,
    int maximumCount)
  {
    int count = port.CountActiveNpcs(npcTypeId);
    if (count < 0)
    {
      throw new InvalidOperationException(
        "The NPC population query returned a negative count.");
    }

    return count >= maximumCount;
  }
}
