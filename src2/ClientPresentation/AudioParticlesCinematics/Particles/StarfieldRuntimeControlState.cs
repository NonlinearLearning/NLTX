namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class StarfieldRuntimeControlState
{
  public bool FallStars { get; private set; }

  public float StarfallIntensity { get; private set; }

  public int ElapsedTicks { get; private set; }

  public void Set(bool fallStars, float starfallIntensity, int elapsedTicks)
  {
    if (float.IsNaN(starfallIntensity) ||
      float.IsInfinity(starfallIntensity) ||
      starfallIntensity < 0 ||
      starfallIntensity > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(starfallIntensity));
    }

    if (elapsedTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedTicks));
    }

    FallStars = fallStars;
    StarfallIntensity = starfallIntensity;
    ElapsedTicks = elapsedTicks;
  }
}
