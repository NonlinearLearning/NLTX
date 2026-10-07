using System;

namespace Terraria.WorldGeneration.Housing;

/// <summary>
/// Evaluates the required furniture categories from the tile-type marks collected by a room scan.
/// </summary>
public static class HousingRoomRequirementsQuery
{
  public static HousingRoomRequirementResult Evaluate(
    ReadOnlySpan<bool> roomTileTypes,
    ReadOnlySpan<int> chairTypes,
    ReadOnlySpan<int> tableTypes,
    ReadOnlySpan<int> torchTypes,
    ReadOnlySpan<int> doorTypes)
  {
    bool hasChair = ContainsAny(roomTileTypes, chairTypes);
    bool hasTable = ContainsAny(roomTileTypes, tableTypes);
    bool hasTorch = ContainsAny(roomTileTypes, torchTypes);
    bool hasDoor = ContainsAny(roomTileTypes, doorTypes);
    return new HousingRoomRequirementResult(
      hasTorch,
      hasDoor,
      hasChair,
      hasTable,
      hasTorch && hasDoor && hasChair && hasTable);
  }

  private static bool ContainsAny(
    ReadOnlySpan<bool> roomTileTypes,
    ReadOnlySpan<int> requiredTypes)
  {
    for (int index = 0; index < requiredTypes.Length; index++)
    {
      if (roomTileTypes[requiredTypes[index]])
      {
        return true;
      }
    }

    return false;
  }
}
