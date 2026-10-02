using System;

namespace Terraria.Npc;

public static class NpcSpawnTypeResolutionSystem
{
  private const int GoodWorldRollRange = 3;
  private const int ManEaterType = 46;
  private const int ManEaterRemixType = 614;
  private const int AngryTrapperType = 62;
  private const int AngryTrapperRemixType = 66;

  public static NpcSpawnTypeResolutionResult Resolve(
    NpcTypeId requestedType,
    bool isGoodWorld,
    INpcSpawnTypeResolutionRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);

    bool rollConsumed = isGoodWorld;
    // The legacy draw precedes type matching, so unmatched types still consume it.
    bool rollPassed = rollConsumed && randomPort.Next(GoodWorldRollRange) != 0;
    NpcTypeId resolvedType = requestedType;
    if (rollPassed)
    {
      if (requestedType.Value == ManEaterType)
      {
        resolvedType = new NpcTypeId(ManEaterRemixType);
      }
      else if (requestedType.Value == AngryTrapperType)
      {
        resolvedType = new NpcTypeId(AngryTrapperRemixType);
      }
    }

    return new NpcSpawnTypeResolutionResult(
      requestedType,
      resolvedType,
      rollConsumed,
      rollPassed);
  }
}
