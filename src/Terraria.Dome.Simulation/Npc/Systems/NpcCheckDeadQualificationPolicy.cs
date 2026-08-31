namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcCheckDeadQualificationPolicy
{
  public static NpcCheckDeadQualificationDecision Evaluate(
    NpcCheckDeadQualificationInput input)
  {
    if (!input.IsNpcActive)
    {
      return new(false, NpcCheckDeadQualificationRejectionReason.Inactive);
    }

    if (!input.IsRootSegment)
    {
      return new(false, NpcCheckDeadQualificationRejectionReason.NonRootSegment);
    }

    if (input.Life > 0)
    {
      return new(false, NpcCheckDeadQualificationRejectionReason.PositiveLife);
    }

    return new(true, NpcCheckDeadQualificationRejectionReason.None);
  }
}
