namespace Terraria.WorldSession.Runtime;

public readonly record struct NpcStreamSchedulingPolicy
{
  public float Speed { get; }

  public NpcStreamSchedulingPolicy(float speed)
  {
    Speed = speed < 0f || float.IsNaN(speed) || float.IsInfinity(speed)
      ? throw new ArgumentOutOfRangeException(nameof(speed))
      : speed;
  }
}
