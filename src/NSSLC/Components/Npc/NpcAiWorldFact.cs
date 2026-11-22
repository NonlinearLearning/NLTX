using System;

namespace Terraria.Npc;

/// <summary>Detached world conditions read by NPC behavior calculations.</summary>
public readonly record struct NpcAiWorldFact
{
  public NpcAiWorldFact(bool dayTime, float worldSurfaceY, bool slimeRain)
  {
    if (!float.IsFinite(worldSurfaceY) || worldSurfaceY < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    IsKnown = true;
    DayTime = dayTime;
    WorldSurfaceY = worldSurfaceY;
    SlimeRain = slimeRain;
  }

  public bool IsKnown { get; }

  public bool DayTime { get; }

  /// <summary>Surface depth in world pixels, matching the NPC position units.</summary>
  public float WorldSurfaceY { get; }

  public bool SlimeRain { get; }
}
