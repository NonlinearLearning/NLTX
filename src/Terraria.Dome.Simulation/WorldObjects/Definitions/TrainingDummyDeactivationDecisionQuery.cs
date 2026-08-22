namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public static class TrainingDummyDeactivationDecisionQuery
{
  public static bool ShouldDeactivate(
    TrainingDummyTileEntityState entity,
    TrainingDummyNpcLinkSnapshot npc)
  {
    return entity.NpcId >= 0 &&
      !TrainingDummyNpcLinkValidityQuery.IsValid(entity, npc);
  }
}
