namespace Terraria.NpcTownBestiary;

public static class BestiaryDisplayIndexQuery
{
  public static bool TryGet(
    IReadOnlyDictionary<NpcNetId, int> displayIndices,
    NpcNetId npcNetId,
    out int displayIndex)
  {
    ArgumentNullException.ThrowIfNull(displayIndices);
    return displayIndices.TryGetValue(npcNetId, out displayIndex);
  }
}
