namespace Terraria.Projectile;

public struct ProjectileTrajectoryStateComponent
{
  public ProjectileTrajectoryStateComponent(
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    float localAi0 = 0.0f,
    float localAi1 = 0.0f,
    float localAi2 = 0.0f,
    float rotation = 0.0f,
    int spriteDirection = 1,
    float stepSpeed = 1.0f,
    int substepCounter = 0)
  {
    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    LocalAi0 = localAi0;
    LocalAi1 = localAi1;
    LocalAi2 = localAi2;
    Rotation = rotation;
    SpriteDirection = spriteDirection;
    StepSpeed = stepSpeed;
    SubstepCounter = substepCounter;
  }

  public float Ai0;
  public float Ai1;
  public float Ai2;
  public float LocalAi0;
  public float LocalAi1;
  public float LocalAi2;
  public float Rotation;
  public int SpriteDirection;
  public float StepSpeed;
  public int SubstepCounter;
}
