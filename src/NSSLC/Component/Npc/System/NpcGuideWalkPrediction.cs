namespace Terraria.Npc;

public readonly record struct NpcGuideWalkTile(
  bool HasLiquid,
  bool IsLava,
  bool IsSolid);

public interface INpcGuideWalkTileQuery
{
  NpcGuideWalkTile ReadTile(int tileX, int tileY);
}

[Flags]
public enum NpcGuideWalkPredictionBranch
{
  None = 0,
  InsideHomeRange = 1 << 0,
  OutsideHomeRange = 1 << 1,
  CrowdKeepWalking = 1 << 2,
  DrowningKeepWalking = 1 << 3,
  TileScan = 1 << 4,
  LavaRisk = 1 << 5,
  SolidLanding = 1 << 6,
  FullLiquidDepth = 1 << 7,
  LandingDrownRisk = 1 << 8,
}

public readonly record struct NpcGuideWalkPredictionInput(
  int TypeId,
  int NetId,
  int AiStyle,
  int MyTileX,
  int HomeFloorX,
  int Direction,
  bool IsTownCritter,
  bool IsLikeTownNpc,
  float Ai1,
  bool CurrentlyDrowning,
  bool CanBreatheUnderWater,
  int TileX,
  int TileY,
  int Width,
  int Height,
  bool SearchAvoidedByNpc,
  bool StationaryFriendlyNpcAhead,
  bool LandingWouldDrown,
  INpcGuideWalkTileQuery? TileQuery);

public readonly record struct NpcGuideWalkPredictionResult(
  bool KeepWalking,
  bool AvoidFalling,
  int LiquidCount,
  NpcGuideWalkPredictionBranch Branches);

/// <summary>
/// Pure source-shaped implementation of
/// AI_007_TownEntities_GetWalkPrediction. The tile and collision owner supplies
/// the tile samples and landing drown result.
/// </summary>
public static class NpcGuideWalkPrediction
{
  public static NpcGuideWalkPredictionResult Evaluate(
    in NpcGuideWalkPredictionInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide walk prediction requires type=22, netID=22, and aiStyle=7.");
    }

    bool keepWalking = false;
    bool avoidFalling = true;
    NpcGuideWalkPredictionBranch branches = NpcGuideWalkPredictionBranch.None;
    bool insideHomeRange = input.MyTileX >= input.HomeFloorX - 35 &&
      input.MyTileX <= input.HomeFloorX + 35;
    branches |= insideHomeRange
      ? NpcGuideWalkPredictionBranch.InsideHomeRange
      : NpcGuideWalkPredictionBranch.OutsideHomeRange;

    if (input.IsLikeTownNpc && input.Ai1 < 30f)
    {
      keepWalking = !input.SearchAvoidedByNpc;
      if (!keepWalking && input.StationaryFriendlyNpcAhead)
      {
        keepWalking = true;
      }

      if (keepWalking)
      {
        branches |= NpcGuideWalkPredictionBranch.CrowdKeepWalking;
      }
    }

    if (!keepWalking && input.CurrentlyDrowning)
    {
      keepWalking = true;
      branches |= NpcGuideWalkPredictionBranch.DrowningKeepWalking;
    }

    if (avoidFalling &&
        (input.IsTownCritter ||
         (!insideHomeRange && input.Direction == Math.Sign(input.HomeFloorX - input.MyTileX))))
    {
      avoidFalling = false;
    }

    if (!avoidFalling)
    {
      return new NpcGuideWalkPredictionResult(
        keepWalking,
        avoidFalling,
        LiquidCount: 0,
        branches);
    }

    ArgumentNullException.ThrowIfNull(input.TileQuery);
    branches |= NpcGuideWalkPredictionBranch.TileScan;
    bool lavaRisk = false;
    bool hasLiquidLanding = false;
    int liquidCount = 0;
    for (int offset = -1; offset <= 4; offset++)
    {
      NpcGuideWalkTile tile = input.TileQuery.ReadTile(
        input.TileX,
        input.TileY + offset);
      if (tile.HasLiquid)
      {
        liquidCount++;
        if (tile.IsLava)
        {
          lavaRisk = true;
          break;
        }
      }

      if (tile.IsSolid)
      {
        if (liquidCount > 0)
        {
          hasLiquidLanding = true;
        }

        avoidFalling = false;
        branches |= NpcGuideWalkPredictionBranch.SolidLanding;
        break;
      }
    }

    if (lavaRisk)
    {
      avoidFalling = true;
      branches |= NpcGuideWalkPredictionBranch.LavaRisk;
    }

    int requiredLiquidDepth = (input.Height + 15) / 16;
    if (liquidCount >= requiredLiquidDepth)
    {
      avoidFalling = true;
      branches |= NpcGuideWalkPredictionBranch.FullLiquidDepth;
    }

    if (!avoidFalling && hasLiquidLanding && input.LandingWouldDrown)
    {
      avoidFalling = true;
      branches |= NpcGuideWalkPredictionBranch.LandingDrownRisk;
    }

    return new NpcGuideWalkPredictionResult(
      keepWalking,
      avoidFalling,
      liquidCount,
      branches);
  }
}
