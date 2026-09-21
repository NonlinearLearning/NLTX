namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class MainFrameTimingComponent
{
  public float UpTimer { get; private set; }

  public float UpTimerMax { get; private set; }

  public float UpTimerMaxDelay { get; private set; }

  public void Configure(float upTimerMax, float upTimerMaxDelay)
  {
    ValidateNonNegative(upTimerMax, nameof(upTimerMax));
    ValidateNonNegative(upTimerMaxDelay, nameof(upTimerMaxDelay));
    UpTimerMax = upTimerMax;
    UpTimerMaxDelay = upTimerMaxDelay;
    UpTimer = MathF.Min(UpTimer, upTimerMax);
  }

  public void Advance(float elapsedSeconds)
  {
    ValidateNonNegative(elapsedSeconds, nameof(elapsedSeconds));
    UpTimer += elapsedSeconds;
    if (UpTimerMax > 0f && UpTimer > UpTimerMax + UpTimerMaxDelay)
    {
      UpTimer = UpTimerMax + UpTimerMaxDelay;
    }
  }

  public void Reset()
  {
    UpTimer = 0f;
  }

  private static void ValidateNonNegative(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
