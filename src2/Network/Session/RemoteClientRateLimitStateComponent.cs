namespace Terraria.Network.Session;

public sealed class RemoteClientRateLimitStateComponent
{
  public RemoteClientRateLimitStateComponent(
    float spamProjectileMax,
    float spamAddBlockMax,
    float spamDeleteBlockMax,
    float spamWaterMax)
  {
    SpamProjectileMax = ValidateMaximum(spamProjectileMax, nameof(spamProjectileMax));
    SpamAddBlockMax = ValidateMaximum(spamAddBlockMax, nameof(spamAddBlockMax));
    SpamDeleteBlockMax = ValidateMaximum(spamDeleteBlockMax, nameof(spamDeleteBlockMax));
    SpamWaterMax = ValidateMaximum(spamWaterMax, nameof(spamWaterMax));
  }

  public float SpamAddBlock { get; private set; }

  public float SpamAddBlockMax { get; }

  public float SpamDeleteBlock { get; private set; }

  public float SpamDeleteBlockMax { get; }

  public float SpamProjectile { get; private set; }

  public float SpamProjectileMax { get; }

  public float SpamWater { get; private set; }

  public float SpamWaterMax { get; }

  private static float ValidateMaximum(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}
