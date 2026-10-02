namespace Terraria.WorldSession.Seeds;

public sealed class WorldPresentationRuleState
{
  public int MoonType { get; private set; }

  public void SetMoonType(int moonType)
  {
    MoonType = moonType;
  }
}
