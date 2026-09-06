namespace Terraria.WorldGeneration.Components;

public sealed class WorldEcologyScheduleState
{
  public bool IsInfectionSpreadAllowed = true;

  public int OvergroundSampleX;
  public int OvergroundSampleD;
  public int UndergroundSampleX;
  public int UndergroundSampleD;

  public int WorldUpdateRate;
  public int EcologyMutationBudget;
  public int TownHousingScanCursor;
  public int PrioritizedTownNpcType = -1;

  public bool IsEcologyPropagationEnabled =>
    IsInfectionSpreadAllowed && WorldUpdateRate > 0;
  public bool HasPrioritizedTownNpc => PrioritizedTownNpcType >= 0;
  public bool HasEcologyBudget => EcologyMutationBudget > 0;
}
