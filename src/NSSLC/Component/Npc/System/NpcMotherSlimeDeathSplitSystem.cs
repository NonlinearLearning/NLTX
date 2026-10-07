using System.Numerics;

namespace Terraria.Npc;

public static class NpcMotherSlimeDeathSplitSystem
{
  public static NpcMotherSlimeDeathSplitResult Apply(
    in NpcMotherSlimeDeathSplitIntent intent,
    Vector2 parentPosition,
    int parentWidth,
    int parentHeight,
    Vector2 parentVelocity,
    int parentDirection,
    int netMode,
    INpcMotherSlimeDeathSplitEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (effectPort.MaxNpcSlots <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(effectPort),
        "The NPC split effect port must expose at least one legacy NPC slot.");
    }

    if (netMode == 1)
    {
      return default;
    }

    int spawnCount = Next(effectPort, 2) + 2;
    int spawnX = (int)(parentPosition.X + parentWidth / 2);
    int spawnY = (int)(parentPosition.Y + parentHeight);
    int spawnCalls = 0;
    int spawnedChildren = 0;
    int syncRequests = 0;
    int rejectedSpawns = 0;

    for (int childIndex = 0; childIndex < spawnCount; childIndex++)
    {
      int npcSlot = effectPort.SpawnBlueSlimeChild(
        intent.SourceNpcInstanceId,
        spawnX,
        spawnY);
      spawnCalls++;

      int horizontalOffset = Next(effectPort, -20, 20);
      int verticalOffset = Next(effectPort, 0, 10);
      int ai0Roll = Next(effectPort, 3);
      Vector2 velocity = new(
        parentVelocity.X * 2f +
          horizontalOffset * 0.1f +
          childIndex * parentDirection * 0.3f,
        parentVelocity.Y - verticalOffset * 0.1f - childIndex);

      if (npcSlot < 0 || npcSlot >= effectPort.MaxNpcSlots)
      {
        rejectedSpawns++;
        continue;
      }

      effectPort.ConfigureBlueSlimeChild(
        npcSlot,
        velocity,
        -1000 * ai0Roll);
      spawnedChildren++;

      if (netMode == 2)
      {
        effectPort.SendNpcSyncPacket(npcSlot);
        syncRequests++;
      }
    }

    return new NpcMotherSlimeDeathSplitResult(
      spawnCalls,
      spawnedChildren,
      syncRequests,
      rejectedSpawns);
  }

  private static int Next(
    INpcMotherSlimeDeathSplitEffectPort effectPort,
    int maxExclusive)
  {
    int value = effectPort.Next(maxExclusive);
    if (value < 0 || value >= maxExclusive)
    {
      throw new InvalidOperationException(
        "The Mother Slime split random port returned a value outside its requested range.");
    }

    return value;
  }

  private static int Next(
    INpcMotherSlimeDeathSplitEffectPort effectPort,
    int minInclusive,
    int maxExclusive)
  {
    int value = effectPort.Next(minInclusive, maxExclusive);
    if (value < minInclusive || value >= maxExclusive)
    {
      throw new InvalidOperationException(
        "The Mother Slime split random port returned a value outside its requested range.");
    }

    return value;
  }
}
