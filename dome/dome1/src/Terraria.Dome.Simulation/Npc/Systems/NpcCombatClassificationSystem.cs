namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcCombatClassificationSystem
{
  private const int MaximumCritterHealth = 5;
  private const int ExcludedCritterDefinitionId = 594;
  private const int SecondExcludedCritterDefinitionId = 686;

  public bool CountsAsCritter(int maximumHealth, int damage, int definitionId)
  {
    return maximumHealth >= 0 && maximumHealth <= MaximumCritterHealth && damage == 0 &&
      definitionId != ExcludedCritterDefinitionId &&
      definitionId != SecondExcludedCritterDefinitionId;
  }

  public bool CanBeChasedBy(
    bool isActive,
    bool isChaseable,
    int maximumHealth,
    bool doesNotTakeDamage,
    bool isFriendly,
    bool isImmortal,
    bool ignoreDoesNotTakeDamage = false,
    bool allowImmortalTargetDummy = false)
  {
    if (!isActive || !isChaseable || maximumHealth <= MaximumCritterHealth || isFriendly)
    {
      return false;
    }

    if (doesNotTakeDamage && !ignoreDoesNotTakeDamage)
    {
      return false;
    }

    return allowImmortalTargetDummy || !isImmortal;
  }
}
