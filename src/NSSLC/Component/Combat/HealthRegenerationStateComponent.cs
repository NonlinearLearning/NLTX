namespace Terraria.Combat;

public struct HealthRegenerationStateComponent
{
  public HealthRegenerationStateComponent(
    int rate,
    int accumulator,
    float timeSinceLastDamage = 0.0f,
    int? expectedLossPerSecond = null)
  {
    Rate = rate;
    Accumulator = accumulator;
    TimeSinceLastDamage = timeSinceLastDamage;
    ExpectedLossPerSecond = expectedLossPerSecond;
  }

  public int Rate;
  public int Accumulator;
  public float TimeSinceLastDamage;
  public int? ExpectedLossPerSecond;

  public bool IsRegenerating => Rate > 0;

  public bool IsDegenerating => Rate < 0;
}
