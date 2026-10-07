using System;

namespace Terraria.Npc;

public static class NpcSpawnGraveyardDualDungeonSelectionSystem
{
  private const int PixelsPerTile = 16;
  private const int SpawnTilePixelOffset = 2;
  private const int DefaultStartIndex = 0;
  private const int DefaultTarget = 255;

  private const int StatueMimicType = 690;
  private const int HardmodeDualDungeonType = 82;
  private const int PreHardmodeDualDungeonType = 316;

  public static NpcSpawnGraveyardDualDungeonSelectionResult Select(
    in NpcSpawnAcceptedCandidate candidate,
    INpcSpawnGraveyardDualDungeonSelectionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    if (!candidate.TileSearchResult.Found)
    {
      return new NpcSpawnGraveyardDualDungeonSelectionResult(
        NpcSpawnGraveyardDualDungeonSelectionStatus.NotApplicable,
        null);
    }

    NpcSpawnRateInputs rateInputs = candidate.RateInputs;
    int tileX = candidate.TileSearchResult.TileX;
    int tileY = candidate.TileSearchResult.TileY;
    int selectedType = 0;

    if (rateInputs.World.DownedBoss3 && rateInputs.BiomeZones.ZoneGraveyard &&
      !candidate.NoWormsForSpawn && port.RollBadLuckExtreme(25) == 0 &&
      !port.HasActiveNpc(StatueMimicType) && port.IsGoodPlaceForStatueMimic(tileX, tileY))
    {
      selectedType = StatueMimicType;
    }
    else if (rateInputs.BiomeAndDungeon.TresspassingDualDungeon && port.RollBadLuck(15) == 0)
    {
      selectedType = rateInputs.World.HardMode
        ? HardmodeDualDungeonType
        : PreHardmodeDualDungeonType;
    }

    if (selectedType == 0)
    {
      return new NpcSpawnGraveyardDualDungeonSelectionResult(
        NpcSpawnGraveyardDualDungeonSelectionStatus.NotApplicable,
        null);
    }

    NpcSpawnEntityRequest request = new(
      Type: new NpcTypeId(selectedType),
      PositionX: tileX * PixelsPerTile + SpawnTilePixelOffset,
      PositionY: tileY * PixelsPerTile,
      StartIndex: DefaultStartIndex,
      Ai0: 0f,
      Ai1: 0f,
      Ai2: 0f,
      Ai3: 0f,
      Target: DefaultTarget);
    return new NpcSpawnGraveyardDualDungeonSelectionResult(
      NpcSpawnGraveyardDualDungeonSelectionStatus.RequestProduced,
      request);
  }
}
