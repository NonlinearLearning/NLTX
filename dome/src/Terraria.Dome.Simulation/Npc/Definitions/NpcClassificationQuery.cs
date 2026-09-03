namespace Terraria.Dome.Simulation.Npc.Definitions;

public static class NpcClassificationQuery
{
  private const int MaximumCritterHealth = 5;
  private const int ExcludedCritterDefinitionId = 594;
  private const int SecondExcludedCritterDefinitionId = 686;

  public static bool CountsAsACritter(NpcDefinition definition, int damage)
  {
    return definition.MaximumHealth >= 0 && definition.MaximumHealth <= MaximumCritterHealth &&
      damage == 0 && definition.DefinitionId != ExcludedCritterDefinitionId &&
      definition.DefinitionId != SecondExcludedCritterDefinitionId;
  }

  public static bool IsLikeATownNpc(NpcDefinition definition)
  {
    return definition.IsLikeTownNpc || definition.Category == NpcCategory.Town;
  }

  public static bool TreatedAsABossForRainbowBoulders(NpcDefinition definition)
  {
    return definition.TreatedAsABossForRainbowBoulders;
  }
}
