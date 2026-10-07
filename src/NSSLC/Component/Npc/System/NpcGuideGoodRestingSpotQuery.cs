namespace Terraria.Npc;

public readonly record struct NpcGuideGoodRestingSpotInput(
  int TypeId,
  int NetId,
  int AiStyle,
  bool DayTime,
  float Ai0,
  bool Wet,
  int TileX,
  int TileY,
  int IdealRestX,
  int IdealRestY);

/// <summary>
/// Exact predicate from AI_007_TownEntities_IsInAGoodRestingSpot for the Guide
/// identity. Finding the ideal floor and the available chair remains an owner
/// query; this type only evaluates the source predicate once those coordinates
/// are supplied.
/// </summary>
public static class NpcGuideGoodRestingSpotQuery
{
  public static bool IsGoodRestingSpot(in NpcGuideGoodRestingSpotInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide resting-spot query requires type=22, netID=22, and aiStyle=7.");
    }

    if (!input.DayTime && input.Ai0 == 5f)
    {
      return Math.Abs(input.TileX - input.IdealRestX) <= 7 &&
        Math.Abs(input.TileY - input.IdealRestY) <= 7;
    }

    return input.TileX == input.IdealRestX &&
      input.TileY == input.IdealRestY;
  }
}
