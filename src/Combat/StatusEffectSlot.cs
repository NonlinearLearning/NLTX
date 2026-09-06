namespace Terraria.Combat;

public struct StatusEffectSlot
{
  public StatusEffectSlot(
    int definitionId,
    int remainingTicks)
  {
    DefinitionId = definitionId;
    RemainingTicks = remainingTicks;
  }

  public int DefinitionId;
  public int RemainingTicks;

  public bool IsOccupied => DefinitionId != 0 && RemainingTicks > 0;
}
