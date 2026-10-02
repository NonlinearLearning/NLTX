namespace Terraria.Npc;

public static class NpcGravitySystem
{
  private const int BalloonNpcType = 258;
  private const int FlyingSnakeNpcType = 425;
  private const int DragonNpcType = 426;
  private const int WyvernHeadNpcType = 427;
  private const int BetsyNpcType = 541;
  private const int EaterOfWorldsHeadNpcType = 576;
  private const int EaterOfWorldsBodyNpcType = 577;
  private const int FloatingAiStyle = 7;

  public static NpcGravityResult Evaluate(
    NpcTypeId npcType,
    NpcAiStateComponent aiState,
    float velocityY,
    in NpcGravityEnvironmentSnapshot environment)
  {
    float gravity = 0.3f;
    float maximumFallSpeed = 10f;
    float velocityYAfter = velocityY;

    if (npcType.Value == BalloonNpcType)
    {
      gravity = 0.1f;
      velocityYAfter = ClampMaximum(velocityYAfter, 3f);
    }
    else if (npcType.Value == FlyingSnakeNpcType && aiState.State2 == 1f)
    {
      gravity = 0.1f;
    }
    else if ((npcType.Value == EaterOfWorldsHeadNpcType ||
              npcType.Value == EaterOfWorldsBodyNpcType) &&
             aiState.State0 > 0f &&
             aiState.State1 == 2f)
    {
      gravity = 0.45f;
      velocityYAfter = ClampMaximum(velocityYAfter, 32f);
    }
    else if (npcType.Value == WyvernHeadNpcType && aiState.State2 == 1f)
    {
      gravity = 0.1f;
      velocityYAfter = ClampMaximum(velocityYAfter, 4f);
    }
    else if (npcType.Value == DragonNpcType)
    {
      gravity = 0.1f;
      velocityYAfter = ClampMaximum(velocityYAfter, 3f);
    }
    else if (npcType.Value == BetsyNpcType ||
             aiState.Style == FloatingAiStyle && aiState.State0 == 25f)
    {
      gravity = 0f;
    }

    float worldScale = (float)environment.WorldMaxTilesX / 4200f;
    worldScale *= worldScale;
    float surfaceScale = (float)((double)(
      environment.NpcPositionY / 16f - (60f + 10f * worldScale)) /
      (environment.WorldSurface / 6.0));
    if ((double)surfaceScale < 0.25)
    {
      surfaceScale = 0.25f;
    }

    if (surfaceScale > 1f)
    {
      surfaceScale = 1f;
    }

    gravity *= surfaceScale;
    if (environment.Wet)
    {
      if (environment.ShimmerWet)
      {
        gravity = 0.15f;
        maximumFallSpeed = 5.5f;
      }
      else if (environment.HoneyWet)
      {
        gravity = 0.1f;
        maximumFallSpeed = 4f;
      }
      else
      {
        gravity = 0.2f;
        maximumFallSpeed = 7f;
      }
    }

    return new NpcGravityResult(gravity, maximumFallSpeed, velocityYAfter);
  }

  private static float ClampMaximum(float value, float maximum)
  {
    return value > maximum ? maximum : value;
  }
}
