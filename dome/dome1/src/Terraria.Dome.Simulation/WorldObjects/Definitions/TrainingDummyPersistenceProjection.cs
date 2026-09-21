namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TrainingDummyPersistenceProjection
{
  public static bool TryProject(
    TileEntityPersistentState source,
    TrainingDummyOwnershipState ownership,
    out TileEntityPersistentState projected)
  {
    if (source.IsOpaque ||
        source.Type != 0 ||
        source.Id != ownership.EntityId ||
        source.TileX != ownership.TileX ||
        source.TileY != ownership.TileY ||
        ownership.Npc.Value > short.MaxValue)
    {
      projected = default!;
      return false;
    }

    short npcId = ownership.Npc.IsValid
      ? checked((short)ownership.Npc.Value)
      : (short)-1;
    projected = new TileEntityPersistentState(
      source.Id,
      source.Type,
      source.TileX,
      source.TileY,
      [(byte)(npcId & 0xFF), (byte)((npcId >> 8) & 0xFF)],
      isOpaque: false);
    return true;
  }
}
