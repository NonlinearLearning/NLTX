namespace Terraria.Npc;

public static class NpcBloodMoonTransformationSystem
{
  public static NpcBloodMoonTransformationIntent? Evaluate(
    bool bloodMoonActive,
    bool crimsonWorld,
    NpcTypeId npcType,
    float npcValue)
  {
    if (!bloodMoonActive)
    {
      return null;
    }

    int targetType = npcType.Value switch
    {
      46 or 303 or 337 or 443 or 540 => crimsonWorld ? 464 : 47,
      55 or 230 or 592 or 593 => crimsonWorld ? 465 : 57,
      148 or 149 => crimsonWorld ? 470 : 168,
      _ => 0,
    };
    if (targetType == 0)
    {
      return null;
    }

    return new NpcBloodMoonTransformationIntent(
      SourceType: npcType,
      TargetType: new NpcTypeId(targetType),
      RestoreValueToZero: npcValue == 0.0f);
  }
}
