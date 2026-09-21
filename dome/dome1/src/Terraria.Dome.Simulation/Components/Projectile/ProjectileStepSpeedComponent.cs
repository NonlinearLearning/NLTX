using System;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileStepSpeedComponent
{
  public ProjectileStepSpeedComponent(float speed = 1.0f)
  {
    if (!float.IsFinite(speed) || speed < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(speed));
    }

    Speed = speed;
  }

  public float Speed { get; set; }

  public float StepSpeed => Speed;
}
