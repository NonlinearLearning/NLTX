using System;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Housing;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>Reads the legacy Main tile grid as explicit housing-validation facts.</summary>
internal sealed class LegacyHousingTileSource : IHousingTileSource, IHousingRoomScoreTileSource
{
  public HousingTileSample ReadTile(TilePosition position)
  {
    var tile = Main.tile[position.X, position.Y];
    bool isActive = tile.nactive();
    int tileType = tile.type;
    bool isHouseWall = Main.wallHouse[tile.wall] ||
      isActive && TileID.Sets.HousingWalls[tileType];
    bool isOpenGate = tileType == 11 &&
        (tile.frameX == 0 || tile.frameX == 54 || tile.frameX == 72 || tile.frameX == 126) ||
      tileType == 389 ||
      tileType == 386 &&
        ((tile.frameX < 36 && tile.frameY == 18) ||
         (tile.frameX >= 36 && tile.frameY == 0));

    return new HousingTileSample(
      isActive,
      isActive && Main.tileSolid[tileType],
      isOpenGate,
      tile.wall,
      isHouseWall,
      isActive && IsRoomRequirementType(
        tileType,
        TileID.Sets.RoomNeeds.CountsAsTorchTypes),
      isActive && IsRoomRequirementType(
        tileType,
        TileID.Sets.RoomNeeds.CountsAsDoorTypes),
      isActive && IsRoomRequirementType(
        tileType,
        TileID.Sets.RoomNeeds.CountsAsChairTypes),
      isActive && IsRoomRequirementType(
        tileType,
        TileID.Sets.RoomNeeds.CountsAsTableTypes),
      isActive && tileType == 630,
      isActive && tileType == 631)
    {
      TileType = tileType,
    };
  }

  HousingRoomScoreTileSample IHousingRoomScoreTileSource.ReadTile(TilePosition position)
  {
    var tile = Main.tile[position.X, position.Y];
    int tileType = tile.type;
    bool isOpenDoorAnchorFrame = WorldGen.IsOpenDoorAnchorFrame(position.X, position.Y);
    int scoreContribution = tileType is 10 or 388 or 389 || isOpenDoorAnchorFrame
      ? -20
      : Main.tileSolid[tileType]
        ? -5
        : 5;

    return new HousingRoomScoreTileSample(
      tile.nactive(),
      Main.tileSolid[tileType],
      TileID.Sets.IgnoredInHouseScore[tileType],
      TileID.Sets.BasicChest[tileType],
      tileType == 11,
      isOpenDoorAnchorFrame,
      GetGoodEvilBalanceContribution(tileType),
      tileType == 379,
      scoreContribution);
  }

  public bool HasSolidTiles(TilePosition minimum, TilePosition maximum)
  {
    return Collision.SolidTiles(
      minimum.X,
      maximum.X,
      minimum.Y,
      maximum.Y);
  }

  private static bool IsRoomRequirementType(int tileType, int[] types)
  {
    return Array.IndexOf(types, tileType) >= 0;
  }

  private static int GetGoodEvilBalanceContribution(int tileType)
  {
    return tileType switch
    {
      109 or 110 or 113 or 117 or 116 or 164 or 403 or 402 => 1,
      23 or 24 or 25 or 32 or 112 or 163 or 400 or 398 or
        199 or 203 or 200 or 401 or 399 or 234 or 352 => -1,
      27 => 5,
      _ => 0,
    };
  }
}
