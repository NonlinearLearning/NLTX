namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TrainingDummyNpcLinkValidityQuery
{
  private const int TrainingDummyNpcType = 488;

  public static bool IsValid(
    TrainingDummyTileEntityState entity,
    TrainingDummyNpcLinkSnapshot npc)
  {
    return entity.NpcId >= 0 &&
      npc.IsActive &&
      npc.NpcType == TrainingDummyNpcType &&
      npc.AiTileX == entity.TileX &&
      npc.AiTileY == entity.TileY;
  }
}
