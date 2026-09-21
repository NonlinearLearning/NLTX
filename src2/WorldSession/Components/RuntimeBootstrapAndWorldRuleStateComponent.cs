namespace Terraria.WorldSession.Components;

public sealed class RuntimeBootstrapAndWorldRuleStateComponent
{
  public int MapDelayTicks { get; private set; }

  public SecretSeedFlags SecretSeedFlags { get; private set; }

  public void Commit(int mapDelayTicks, SecretSeedFlags secretSeedFlags)
  {
    if (mapDelayTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mapDelayTicks));
    }

    MapDelayTicks = mapDelayTicks;
    SecretSeedFlags = secretSeedFlags;
  }

  public void Clear()
  {
    MapDelayTicks = 0;
    SecretSeedFlags = default;
  }
}
