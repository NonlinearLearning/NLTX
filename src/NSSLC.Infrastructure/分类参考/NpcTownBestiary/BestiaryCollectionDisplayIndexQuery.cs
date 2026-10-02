namespace Terraria.NpcTownBestiary;

public static class BestiaryCollectionDisplayIndexQuery
{
  public static bool TryGet(
    IReadOnlyDictionary<NpcNetId, int> displayIndices,
    NpcNetId npcNetId,
    out int displayIndex)
  {
    return BestiaryDisplayIndexQuery.TryGet(displayIndices, npcNetId, out displayIndex);
  }
}
